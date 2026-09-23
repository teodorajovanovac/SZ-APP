using Microsoft.EntityFrameworkCore;
using SzApp.Data;

namespace SzApp.Worker;

public sealed class OutboxWorker(
    IDbContextFactory<SzAppDbContext> dbContextFactory,
    IEnumerable<IOutboxMessageHandler> handlers,
    TimeProvider timeProvider,
    ILogger<OutboxWorker> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private readonly Dictionary<string, IOutboxMessageHandler> _handlers =
        handlers.ToDictionary(x => x.MessageType, StringComparer.OrdinalIgnoreCase);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Outbox batch processing failed.");
            }

            await Task.Delay(PollInterval, timeProvider, stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // UPDLOCK+READPAST is the standard SQL Server work-queue-poller pattern (mirrors
            // JournalPostingService/BankStatementService's UPDLOCK/HOLDLOCK claim-before-act use): a
            // second worker instance's SELECT skips rows this transaction already locked, instead of
            // blocking on them, so two instances can't grab and send the same message.
            // ponytail: the lock is held for the whole batch, including handler I/O (SMTP send) —
            // fine at this volume (<=20 msgs/5s poll); if throughput or handler latency grows, switch
            // to a short claim transaction (mark+commit) followed by unlocked processing.
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            var now = timeProvider.GetUtcNow();
            // ponytail: OutboxMessage (ops schema, owned outside this feature's scope) has no direct
            // SentEmailId/NextAttemptAt of its own, so backoff is looked up by parsing the "email:{id}"
            // DedupeKey convention EmailEndpoints.cs writes. Ceiling: only works for the email message
            // type and breaks if that convention changes. Upgrade: add a NextAttemptAt column (or a
            // typed reference) directly on OutboxMessage if a second backoff-aware message type appears.
            var messages = await dbContext.OutboxMessages.FromSqlInterpolated($"""
                SELECT TOP (20) om.*
                FROM [ops].[OutboxMessage] om WITH (UPDLOCK, READPAST)
                OUTER APPLY (
                    SELECT TOP (1) se.[NextAttemptAt] AS NextAttemptAt
                    FROM [platform].[SentEmail] se
                    WHERE om.[Type] = N'platform.email.send'
                      AND se.[Id] = TRY_CAST(SUBSTRING(om.[DedupeKey], 7, 120) AS BIGINT)
                ) linked
                WHERE om.[ProcessedAt] IS NULL
                  AND om.[AttemptCount] < 10
                  AND (linked.[NextAttemptAt] IS NULL OR linked.[NextAttemptAt] <= {now})
                ORDER BY om.[OccurredAt]
                """).ToListAsync(cancellationToken);

            foreach (var message in messages)
            {
                if (!_handlers.TryGetValue(message.Type, out var handler))
                {
                    message.AttemptCount++;
                    message.LastError = "No outbox handler registered for this message type.";
                    logger.LogWarning("No outbox handler registered for {MessageType}; message {MessageId} attempt {AttemptCount}.",
                        message.Type, message.Id, message.AttemptCount);
                    continue;
                }

                try
                {
                    await handler.HandleAsync(message.PayloadJson, cancellationToken);
                    message.ProcessedAt = timeProvider.GetUtcNow();
                    message.LastError = null;
                }
                catch (Exception exception)
                {
                    message.AttemptCount++;
                    message.LastError = exception.Message[..Math.Min(exception.Message.Length, 2_000)];
                    logger.LogWarning(exception, "Outbox message {MessageId} failed on attempt {AttemptCount}.", message.Id, message.AttemptCount);
                }
            }

            if (messages.Count > 0)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        });
    }
}

public interface IOutboxMessageHandler
{
    string MessageType { get; }

    Task HandleAsync(string payloadJson, CancellationToken cancellationToken);
}
