using System.Globalization;
using System.Text.Json;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Data.Entities.EtlExtended;
using SzApp.Data.Entities.LedgerBanking;
using SzApp.Domain;
using SzApp.Etl.Csv;
using SzApp.Etl.Parsing;

namespace SzApp.Etl.Pipeline;

public interface IEtlPipelineService
{
    Task ValidateAndStageAsync(Guid runId, string absoluteFilePath, string encodingName, char delimiter, CancellationToken cancellationToken);
    Task ExecuteAsync(Guid runId, CancellationToken cancellationToken);
}

public sealed class EtlPipelineService(
    SzAppDbContext dbContext,
    LegacyCsvParser csvParser,
    TimeProvider timeProvider) : IEtlPipelineService
{
    public const string PipelineVersion = "1.0";

    public async Task ValidateAndStageAsync(
        Guid runId,
        string absoluteFilePath,
        string encodingName,
        char delimiter,
        CancellationToken cancellationToken)
    {
        var (run, context) = await GetRunAsync(runId, cancellationToken);
        if (context.Stage >= EtlPipelineStage.Validated && context.Stage != EtlPipelineStage.Failed)
        {
            return;
        }

        try
        {
            await using var file = File.OpenRead(absoluteFilePath);
            var document = await csvParser.ParseAsync(file, encodingName, delimiter, cancellationToken);
            var now = timeProvider.GetUtcNow();

            foreach (var row in document.Rows)
            {
                var raw = new RawStagingRow
                {
                    EtlRunId = runId,
                    SourceSystem = run.SourceSystem,
                    SourceTable = context.SourceTable,
                    SourceRowNumber = row.RowNumber,
                    SourceKey = row.Values.GetValueOrDefault("Id"),
                    RowHash = row.RowHash,
                    RawText = row.RawText,
                    ValuesJson = JsonSerializer.Serialize(row.Values),
                    StagedAt = now
                };
                dbContext.Set<RawStagingRow>().Add(raw);

                foreach (var error in GenericRowValidator.Validate(row.Values))
                {
                    dbContext.QuarantineRecords.Add(new QuarantineRecord
                    {
                        EtlRunId = runId,
                        SourceTable = context.SourceTable,
                        SourceKey = raw.SourceKey,
                        RawJson = raw.ValuesJson,
                        ErrorCode = error.Code,
                        ErrorMessage = error.Message,
                        CreatedAt = now
                    });
                }
            }

            run.SourceRowCount = document.Rows.Count;
            run.QuarantinedRowCount = await dbContext.QuarantineRecords.CountAsync(x => x.EtlRunId == runId, cancellationToken)
                + dbContext.ChangeTracker.Entries<QuarantineRecord>().Count(x => x.Entity.EtlRunId == runId && x.State == EntityState.Added);
            run.Status = EtlRunStatus.Pending;
            context.EncodingName = encodingName;
            context.Delimiter = delimiter.ToString();
            context.Stage = EtlPipelineStage.Staged;
            context.UpdatedAt = now;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is InvalidDataException or DecoderFallbackException or FormatException)
        {
            run.Status = EtlRunStatus.Failed;
            run.Error = exception.Message;
            context.Stage = EtlPipelineStage.Failed;
            context.UpdatedAt = timeProvider.GetUtcNow();
            await dbContext.SaveChangesAsync(cancellationToken);
            throw;
        }
    }

    public async Task ExecuteAsync(Guid runId, CancellationToken cancellationToken)
    {
        var (run, context) = await GetRunAsync(runId, cancellationToken);
        if (context.Stage == EtlPipelineStage.Completed)
        {
            return;
        }

        if (context.Stage != EtlPipelineStage.Staged)
        {
            throw new DomainRuleException("etl.not-staged", "Import mora biti validiran i smešten u staging pre izvršavanja.");
        }

        await EnsureDependenciesAsync(context, cancellationToken);
        run.Status = EtlRunStatus.Running;
        context.Stage = EtlPipelineStage.Importing;
        context.UpdatedAt = timeProvider.GetUtcNow();
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            var quarantinedKeys = await dbContext.QuarantineRecords.AsNoTracking()
                .Where(x => x.EtlRunId == runId && x.SourceKey != null)
                .Select(x => x.SourceKey!)
                .ToHashSetAsync(cancellationToken);
            var rows = await dbContext.Set<RawStagingRow>().AsNoTracking()
                .Where(x => x.EtlRunId == runId)
                .OrderBy(x => x.SourceRowNumber)
                .ToArrayAsync(cancellationToken);
            var mappedKeys = await dbContext.LegacyKeyMaps.AsNoTracking()
                .Where(x => x.EtlRunId == runId)
                .Select(x => x.SourceKey)
                .ToHashSetAsync(cancellationToken);

            foreach (var row in rows.Where(x => !quarantinedKeys.Contains(x.SourceKey ?? string.Empty)))
            {
                var sourceKey = row.SourceKey ?? row.SourceRowNumber.ToString(CultureInfo.InvariantCulture);
                if (!mappedKeys.Add(sourceKey))
                {
                    continue;
                }

                dbContext.LegacyKeyMaps.Add(new LegacyKeyMap
                {
                    EtlRunId = runId,
                    SourceTable = context.SourceTable,
                    SourceKey = sourceKey,
                    TargetTable = $"staging:{context.SourceTable}",
                    TargetKey = row.Id.ToString(CultureInfo.InvariantCulture)
                });
                AddRelationships(runId, context.SourceTable, sourceKey, row.ValuesJson);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await ResolveRelationshipsAsync(context.CompanyId, runId, cancellationToken);

            context.Stage = EtlPipelineStage.Reconciling;
            await ReconcileAsync(run, context, cancellationToken);
            AddBlockedParityAssessments(runId);

            run.ImportedRowCount = await dbContext.LegacyKeyMaps.CountAsync(x => x.EtlRunId == runId, cancellationToken);
            run.QuarantinedRowCount = await dbContext.QuarantineRecords.CountAsync(x => x.EtlRunId == runId, cancellationToken);
            run.Status = EtlRunStatus.Completed;
            run.CompletedAt = timeProvider.GetUtcNow();
            context.Stage = EtlPipelineStage.Completed;
            context.UpdatedAt = timeProvider.GetUtcNow();
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            run.Status = EtlRunStatus.Failed;
            run.Error = exception.Message;
            context.Stage = EtlPipelineStage.Failed;
            context.UpdatedAt = timeProvider.GetUtcNow();
            await dbContext.SaveChangesAsync(cancellationToken);
            throw;
        }
    }

    private async Task EnsureDependenciesAsync(EtlRunContext context, CancellationToken cancellationToken)
    {
        var node = LegacyImportTopology.Find(context.SourceTable);
        if (node is null)
        {
            return;
        }

        var completedTables = await dbContext.Set<EtlRunContext>().AsNoTracking()
            .Where(x => x.CompanyId == context.CompanyId && x.Stage == EtlPipelineStage.Completed)
            .Select(x => x.SourceTable)
            .ToArrayAsync(cancellationToken);
        var missing = node.Dependencies.Except(completedTables, StringComparer.OrdinalIgnoreCase).ToArray();
        if (missing.Length > 0)
        {
            throw new DomainRuleException(
                "etl.dependencies-missing",
                $"Pre tabele {context.SourceTable} moraju biti završene: {string.Join(", ", missing)}.");
        }
    }

    private void AddRelationships(Guid runId, string sourceTable, string sourceKey, string valuesJson)
    {
        var values = JsonSerializer.Deserialize<Dictionary<string, string>>(valuesJson)
            ?? new Dictionary<string, string>();
        foreach (var (field, value) in values.Where(x => x.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase) &&
                                                         !string.Equals(x.Key, "Id", StringComparison.OrdinalIgnoreCase)))
        {
            var targetKey = SerbianLegacyValueParser.ZeroToNull(value);
            if (targetKey is null)
            {
                continue;
            }

            dbContext.Set<PendingLegacyRelationship>().Add(new PendingLegacyRelationship
            {
                EtlRunId = runId,
                SourceTable = sourceTable,
                SourceKey = sourceKey,
                RelationshipName = field,
                TargetSourceTable = ResolveTargetTable(field),
                TargetSourceKey = targetKey
            });
        }
    }

    private async Task ResolveRelationshipsAsync(int companyId, Guid runId, CancellationToken cancellationToken)
    {
        var pending = await dbContext.Set<PendingLegacyRelationship>()
            .Where(x => x.EtlRunId == runId && !x.IsResolved)
            .ToArrayAsync(cancellationToken);
        foreach (var relationship in pending)
        {
            var targetExists = await (
                from map in dbContext.LegacyKeyMaps.AsNoTracking()
                join runContext in dbContext.Set<EtlRunContext>().AsNoTracking() on map.EtlRunId equals runContext.EtlRunId
                where runContext.CompanyId == companyId &&
                      map.SourceTable == relationship.TargetSourceTable &&
                      map.SourceKey == relationship.TargetSourceKey
                select map.Id).AnyAsync(cancellationToken);
            relationship.IsResolved = targetExists;
            relationship.ResolutionError = targetExists
                ? null
                : $"Nije pronađen {relationship.TargetSourceTable}:{relationship.TargetSourceKey}.";
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ReconcileAsync(EtlRun run, EtlRunContext context, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var mappedCount = await dbContext.LegacyKeyMaps.CountAsync(x => x.EtlRunId == run.Id, cancellationToken);
        AddReconciliation(run.Id, ReconciliationMetric.RowCount, context.SourceTable, run.SourceRowCount, mappedCount, now);

        var orphanCount = await dbContext.Set<PendingLegacyRelationship>()
            .CountAsync(x => x.EtlRunId == run.Id && !x.IsResolved, cancellationToken);
        AddReconciliation(run.Id, ReconciliationMetric.OrphanCount, context.SourceTable, 0m, orphanCount, now);

        var rawRows = await dbContext.Set<RawStagingRow>().AsNoTracking()
            .Where(x => x.EtlRunId == run.Id)
            .Select(x => x.ValuesJson)
            .ToArrayAsync(cancellationToken);
        if (context.SourceTable.Equals("Invoice", StringComparison.OrdinalIgnoreCase))
        {
            var expected = SumJsonField(rawRows, "InvoiceTotal");
            var actual = await dbContext.Invoices.Where(x => x.CompanyId == context.CompanyId)
                .SumAsync(x => x.InvoiceTotal, cancellationToken);
            AddReconciliation(run.Id, ReconciliationMetric.InvoiceTotal, "Invoice", expected, actual, now);
        }
        else if (context.SourceTable.Equals("LedgerEntry", StringComparison.OrdinalIgnoreCase))
        {
            var expected = SumJsonField(rawRows, "DebitAmount") - SumJsonField(rawRows, "CreditAmount");
            var actual = await dbContext.LedgerEntries.Where(x => x.CompanyId == context.CompanyId)
                .SumAsync(x => x.DebitAmount - x.CreditAmount, cancellationToken);
            AddReconciliation(run.Id, ReconciliationMetric.TrialBalance, "LedgerEntry", expected, actual, now);
        }
        else if (context.SourceTable.Equals("BankStatement", StringComparison.OrdinalIgnoreCase))
        {
            var expectedDebit = SumJsonField(rawRows, "Debit");
            var expectedCredit = SumJsonField(rawRows, "Credit");
            var actualDebit = await dbContext.Set<BankStatement>().Where(x => x.CompanyId == context.CompanyId)
                .SumAsync(x => x.Debit, cancellationToken);
            var actualCredit = await dbContext.Set<BankStatement>().Where(x => x.CompanyId == context.CompanyId)
                .SumAsync(x => x.Credit, cancellationToken);
            AddReconciliation(run.Id, ReconciliationMetric.BankDebit, "BankStatement", expectedDebit, actualDebit, now);
            AddReconciliation(run.Id, ReconciliationMetric.BankCredit, "BankStatement", expectedCredit, actualCredit, now);
        }
    }

    private void AddBlockedParityAssessments(Guid runId)
    {
        var now = timeProvider.GetUtcNow();
        foreach (var (kind, expected) in new[] { ("Queries", 898), ("Forms", 136), ("Reports", 124), ("VbaModules", 42) })
        {
            dbContext.Set<EtlParityAssessment>().Add(new EtlParityAssessment
            {
                EtlRunId = runId,
                ArtifactKind = kind,
                Status = LegacyParityStatus.BlockedByMissingSource,
                ExpectedCount = expected,
                AssessedCount = 0,
                Reason = "legacy-source/ nije dostupan; 1:1 Access paritet ne može biti potvrđen.",
                AssessedAt = now
            });
        }
    }

    private void AddReconciliation(
        Guid runId,
        ReconciliationMetric metric,
        string scope,
        decimal expected,
        decimal actual,
        DateTimeOffset checkedAt) =>
        dbContext.Set<ReconciliationRecord>().Add(new ReconciliationRecord
        {
            EtlRunId = runId,
            Metric = metric,
            Scope = scope,
            ExpectedValue = expected,
            ActualValue = actual,
            IsMatch = expected == actual,
            CheckedAt = checkedAt
        });

    private static decimal SumJsonField(IEnumerable<string> rows, string field)
    {
        decimal total = 0m;
        foreach (var json in rows)
        {
            var values = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (values?.TryGetValue(field, out var raw) == true && !string.IsNullOrWhiteSpace(raw))
            {
                total += SerbianLegacyValueParser.ParseDecimal(raw);
            }
        }

        return total;
    }

    private async Task<(EtlRun Run, EtlRunContext Context)> GetRunAsync(Guid runId, CancellationToken cancellationToken)
    {
        var run = await dbContext.EtlRuns.SingleOrDefaultAsync(x => x.Id == runId, cancellationToken)
            ?? throw new KeyNotFoundException("ETL run nije pronađen.");
        var context = await dbContext.Set<EtlRunContext>().SingleAsync(x => x.EtlRunId == runId, cancellationToken);
        return (run, context);
    }

    private static string ResolveTargetTable(string relationshipName)
    {
        var withoutId = relationshipName[..^2];
        return withoutId switch
        {
            "OwnerPartner" or "InvoicePartner" or "TenantPartner" or "Manager" => "Partner",
            "InvoiceDeliveryUnit" => "Unit",
            "JournalEntry" => "JournalEntry",
            _ => withoutId
        };
    }
}

public sealed record RowValidationError(string Code, string Message);

public static class GenericRowValidator
{
    public static IReadOnlyCollection<RowValidationError> Validate(IReadOnlyDictionary<string, string> values)
    {
        var errors = new List<RowValidationError>();
        foreach (var (field, value) in values.Where(x => !string.IsNullOrWhiteSpace(x.Value)))
        {
            try
            {
                if (field.EndsWith("Date", StringComparison.OrdinalIgnoreCase) || field == "Date")
                {
                    SerbianLegacyValueParser.ParseDate(value);
                }
                else if (field.StartsWith("Is", StringComparison.OrdinalIgnoreCase) ||
                         field.StartsWith("Has", StringComparison.OrdinalIgnoreCase))
                {
                    SerbianLegacyValueParser.ParseAccessBoolean(value);
                }
                else if (IsDecimalField(field))
                {
                    SerbianLegacyValueParser.ParseDecimal(value);
                }
                else if (field.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                {
                    SerbianLegacyValueParser.ParseNullableForeignKey(value);
                }
            }
            catch (FormatException exception)
            {
                errors.Add(new RowValidationError("etl.invalid-value", $"Polje {field}: {exception.Message}"));
            }
        }

        return errors;
    }

    private static bool IsDecimalField(string field) =>
        field.Contains("Amount", StringComparison.OrdinalIgnoreCase) ||
        field is "Debit" or "Credit" or "Total" or "Balance" or "Rate" or "K1" or "K2" or "K3" or "K4" or "K5";
}
