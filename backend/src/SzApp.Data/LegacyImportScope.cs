using Microsoft.EntityFrameworkCore;
using SzApp.Data.Configurations.LedgerBanking;

namespace SzApp.Data;

/// <summary>
/// ETL-06: the one sanctioned way around the posting guards, for importing historical (already
/// posted) legacy GL. Holds the connection open for the scope — SESSION_CONTEXT is per session and
/// pooled connections reset it — sets szapp_etl_import = 1 for the DB triggers, and flags the
/// async flow so the EF LedgerMutationGuardInterceptor stands down too. Only the ETL pipeline
/// (Root-only endpoint) opens it.
/// </summary>
public static class LegacyImportScope
{
    private static readonly AsyncLocal<int> Depth = new();

    public static bool IsActive => Depth.Value > 0;

    // Not async on purpose: an AsyncLocal written inside an async method doesn't flow back to the
    // caller, so the flag flips here (and in DisposeAsync) synchronously, in the caller's context.
    public static Task<IAsyncDisposable> BeginAsync(SzAppDbContext db, CancellationToken ct)
    {
        Depth.Value++;
        return OpenAsync(new Lease(db, db.Database.IsSqlServer()), ct);
    }

    private static async Task<IAsyncDisposable> OpenAsync(Lease lease, CancellationToken ct)
    {
        await lease.OpenAsync(ct);
        return lease;
    }

    private static Task SetAsync(SzAppDbContext db, int? value, CancellationToken ct) =>
        db.Database.ExecuteSqlRawAsync(
            "EXEC sys.sp_set_session_context @key = {0}, @value = {1}",
            [LedgerBankingDbGuardSql.EtlImportSessionKey, (object?)value ?? DBNull.Value],
            ct);

    private sealed class Lease(SzAppDbContext db, bool isSqlServer) : IAsyncDisposable
    {
        private bool disposed;

        public async Task OpenAsync(CancellationToken ct)
        {
            if (!isSqlServer) return;
            await db.Database.OpenConnectionAsync(ct);
            await SetAsync(db, 1, ct);
        }

        public ValueTask DisposeAsync()
        {
            if (disposed) return ValueTask.CompletedTask;
            disposed = true;
            Depth.Value--;
            return isSqlServer ? new ValueTask(CloseAsync()) : ValueTask.CompletedTask;
        }

        private async Task CloseAsync()
        {
            try
            {
                await SetAsync(db, null, CancellationToken.None);
            }
            finally
            {
                await db.Database.CloseConnectionAsync();
            }
        }
    }
}
