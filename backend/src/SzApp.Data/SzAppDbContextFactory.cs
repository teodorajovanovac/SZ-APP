using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SzApp.Data;

public sealed class SzAppDbContextFactory : IDesignTimeDbContextFactory<SzAppDbContext>
{
    public SzAppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("SZAPP_CONNECTION_STRING")
            ?? "Server=localhost,1433;Database=SzApp;User Id=sa;Password=Local_dev_only_123!;TrustServerCertificate=True;Encrypt=True";

        var options = new DbContextOptionsBuilder<SzAppDbContext>()
            .UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure())
            .Options;

        return new SzAppDbContext(options);
    }
}
