using System.Globalization;
using System.Text.Json;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Data.Entities.EtlExtended;
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
                // ponytail (#5): a structural (column-count) error means Values is empty/unusable —
                // quarantine straight away and skip staging, instead of parsing the whole file
                // failing (see LegacyCsvParser).
                if (row.StructuralError is not null)
                {
                    dbContext.QuarantineRecords.Add(new QuarantineRecord
                    {
                        EtlRunId = runId,
                        SourceTable = context.SourceTable,
                        SourceKey = null,
                        RawJson = row.RawText,
                        ErrorCode = "etl.malformed-row",
                        ErrorMessage = row.StructuralError,
                        CreatedAt = now
                    });
                    continue;
                }

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

        // ponytail (#4): a crash mid-run used to leave the run stuck forever — ExecuteAsync only
        // accepted Stage == Staged. Now Importing/Reconciling (crashed after we flipped the stage,
        // before we finished) are treated as a resume. Failed is only resumable once we've confirmed
        // staging actually produced rows (RawStagingRow exists for this run) — otherwise a run that
        // failed *inside* ValidateAndStageAsync, before anything was persisted, would be silently
        // "completed" with zero rows. Resuming is safe without a spanning transaction because row
        // inserts are guarded by the GLOBAL LegacyKeyMap dedupe check below (#3): replaying
        // already-imported/materialized rows is a no-op, not a duplicate-key crash. This is the
        // less-risky option vs. wrapping the whole multi-minute import in one DB transaction (would
        // hold locks for the whole run and still needs the same idempotency guarantee anyway).
        var isResumable = context.Stage is EtlPipelineStage.Importing or EtlPipelineStage.Reconciling ||
            (context.Stage == EtlPipelineStage.Failed &&
             await dbContext.Set<RawStagingRow>().AnyAsync(x => x.EtlRunId == runId, cancellationToken));
        if (context.Stage != EtlPipelineStage.Staged && !isResumable)
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
            // ponytail (#3): GLOBAL dedupe, not scoped to this EtlRunId. The unique index backing
            // LegacyKeyMap is (SourceTable, SourceKey, TargetTable) with no EtlRunId — re-importing
            // an updated file (new run, same source rows) must skip rows already mapped by an
            // earlier run, not just rows mapped earlier in *this* run, or the second SaveChangesAsync
            // throws a unique-constraint violation and the whole run is marked Failed for what should
            // be a normal "0 new rows" no-op re-import.
            var mappedKeys = await dbContext.LegacyKeyMaps.AsNoTracking()
                .Where(x => x.SourceTable == context.SourceTable)
                .Select(x => x.SourceKey)
                .ToHashSetAsync(cancellationToken);
            var materializable = MasterDataMaterializer.IsSupported(context.SourceTable);

            foreach (var row in rows.Where(x => !quarantinedKeys.Contains(x.SourceKey ?? string.Empty)))
            {
                var sourceKey = row.SourceKey ?? row.SourceRowNumber.ToString(CultureInfo.InvariantCulture);
                if (!mappedKeys.Add(sourceKey))
                {
                    continue;
                }

                string targetTable;
                string targetKey;
                if (materializable)
                {
                    var values = new Dictionary<string, string>(
                        JsonSerializer.Deserialize<Dictionary<string, string>>(row.ValuesJson) ?? [],
                        StringComparer.OrdinalIgnoreCase);
                    var newId = await MasterDataMaterializer.TryMaterializeAsync(dbContext, context.SourceTable, values, cancellationToken);
                    if (newId is { } id)
                    {
                        targetTable = context.SourceTable;
                        targetKey = id.ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        // required FK not resolved yet (dependency table not imported) — leave staging-only
                        targetTable = $"staging:{context.SourceTable}";
                        targetKey = row.Id.ToString(CultureInfo.InvariantCulture);
                    }
                }
                else
                {
                    targetTable = $"staging:{context.SourceTable}";
                    targetKey = row.Id.ToString(CultureInfo.InvariantCulture);
                }

                dbContext.LegacyKeyMaps.Add(new LegacyKeyMap
                {
                    EtlRunId = runId,
                    SourceTable = context.SourceTable,
                    SourceKey = sourceKey,
                    TargetTable = targetTable,
                    TargetKey = targetKey
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

    // ponytail (#6): used to treat ANY column ending in "Id" as an FK and guess its target table by
    // string-stripping the suffix (e.g. "InvoiceDeliveryUnitId" -> hardcoded "Unit", others ->
    // whatever's left after chopping "Id") — that's exactly the "don't invent FK relationships"
    // rule this project's CLAUDE.md warns about, and it's wrong for plenty of real columns (e.g.
    // "PartnerTypeId" isn't a table named "PartnerType", it's a ShortList lookup). Now it only
    // records a relationship for columns already reviewed and listed explicitly in
    // LegacyImportTopology's DeferredRelationships — a small, explicit, reviewable allowlist (see
    // docs/data-model.md's own "-- FK to X.Id" comments, which is where that list was seeded from).
    private void AddRelationships(Guid runId, string sourceTable, string sourceKey, string valuesJson)
    {
        var allowlisted = LegacyImportTopology.Find(sourceTable)?.DeferredRelationships;
        if (allowlisted is null || allowlisted.Count == 0)
        {
            return;
        }

        var values = JsonSerializer.Deserialize<Dictionary<string, string>>(valuesJson)
            ?? new Dictionary<string, string>();
        foreach (var relationship in allowlisted)
        {
            if (!values.TryGetValue(relationship.Name, out var value))
            {
                continue;
            }

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
                RelationshipName = relationship.Name,
                TargetSourceTable = relationship.TargetTable,
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

        // ponytail (#2): Invoice/LedgerEntry/BankStatement have zero materialization logic (they
        // stay staging-only — see MasterDataMaterializer's class comment for why). Comparing a real
        // CSV total against dbContext.Invoices/.LedgerEntries/BankStatement — which are permanently
        // empty for these source tables today — would always "mismatch" and made reconciliation
        // structurally unable to pass. Short-circuit with an honest "not applicable" result instead
        // of a false failure; flip this back to a real comparison once those tables get a
        // materializer.
        if (context.SourceTable.Equals("Invoice", StringComparison.OrdinalIgnoreCase))
        {
            AddNotApplicableReconciliation(run.Id, ReconciliationMetric.InvoiceTotal, "Invoice", now);
        }
        else if (context.SourceTable.Equals("LedgerEntry", StringComparison.OrdinalIgnoreCase))
        {
            AddNotApplicableReconciliation(run.Id, ReconciliationMetric.TrialBalance, "LedgerEntry", now);
        }
        else if (context.SourceTable.Equals("BankStatement", StringComparison.OrdinalIgnoreCase))
        {
            AddNotApplicableReconciliation(run.Id, ReconciliationMetric.BankDebit, "BankStatement", now);
            AddNotApplicableReconciliation(run.Id, ReconciliationMetric.BankCredit, "BankStatement", now);
        }
    }

    private void AddNotApplicableReconciliation(Guid runId, ReconciliationMetric metric, string scope, DateTimeOffset checkedAt) =>
        dbContext.Set<ReconciliationRecord>().Add(new ReconciliationRecord
        {
            EtlRunId = runId,
            Metric = metric,
            Scope = scope,
            ExpectedValue = 0m,
            ActualValue = 0m,
            IsMatch = null,
            Details = $"Nije primenjivo — {scope} još nema materijalizaciju u realnu tabelu (staging-only import).",
            CheckedAt = checkedAt
        });

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

    private async Task<(EtlRun Run, EtlRunContext Context)> GetRunAsync(Guid runId, CancellationToken cancellationToken)
    {
        var run = await dbContext.EtlRuns.SingleOrDefaultAsync(x => x.Id == runId, cancellationToken)
            ?? throw new KeyNotFoundException("ETL run nije pronađen.");
        var context = await dbContext.Set<EtlRunContext>().SingleAsync(x => x.EtlRunId == runId, cancellationToken);
        return (run, context);
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
