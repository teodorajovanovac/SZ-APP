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
        var messages = await dbContext.OutboxMessages
            .Where(x => x.ProcessedAt == null && x.AttemptCount < 10)
            .OrderBy(x => x.OccurredAt)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            if (!_handlers.TryGetValue(message.Type, out var handler))
            {
                logger.LogDebug("No outbox handler registered for {MessageType}; message {MessageId} remains pending.", message.Type, message.Id);
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
    }
}

public interface IOutboxMessageHandler
{
    string MessageType { get; }

    Task HandleAsync(string payloadJson, CancellationToken cancellationToken);
}
