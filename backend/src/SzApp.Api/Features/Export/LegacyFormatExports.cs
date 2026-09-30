using System.Data.Common;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SzApp.Api.Security;
using SzApp.Data;
using SzApp.Data.Entities;
using SzApp.Etl.Export;

namespace SzApp.Api.Features.Export;

public sealed record LegacyExportResponse(string Name, string Title, bool NeedsPeriod);

/// <summary>
/// GAP-21: the legacy EXPORT001..008 / SPISAK-STANARA-EXPORT queries as ';' CSV (cp1250 or UTF-8 BOM, opens
/// in Excel) with the legacy column names. Mapping: Skustina.NazivSS = company partner name, Zgrada =
/// Company.ShortName, Objekti.Status = 1 = active contract, Kupac = the contract's 2040 PartnerAccount.
/// Not ported: EXPORT002-KONTROLA-003 (legacy data check), EXPORT003A (ZK staging) and Export004 (benefit line archive).
/// </summary>
public static class LegacyFormatExports
{
    private const string Ids = "(SELECT CAST([value] AS int) FROM OPENJSON(@ids))";

    private const string UnitBase =
        "FROM core.Unit u JOIN core.Company c ON c.Id = u.CompanyId LEFT JOIN core.Partner cp ON cp.Id = c.PartnerId " +
        "LEFT JOIN core.Contract k ON k.Id = u.ContractId " +
        "LEFT JOIN core.PartnerAccount pa ON pa.ContractId = k.Id AND pa.Account = '2040' LEFT JOIN core.Partner p ON p.Id = pa.PartnerId " +
        "LEFT JOIN core.ShortList t ON t.Id = u.UnitTypeId LEFT JOIN core.BuildingEntrance be ON be.Id = u.BuildingEntranceId ";

    private const string UnitListing =
        "SELECT c.Id AS IDSZ, cp.Name AS [Stambena zajednica], c.ShortName AS Zgrada, be.EntranceName AS Ulaz, t.Caption AS [Vrsta posebnog dela], " +
        "u.Id AS SifraPD, p.Name AS Vlasnik, u.K1 AS [K1 - m2], u.K2 AS [K2 - keoficijent], u.K3 AS [K3 - broj], u.K4 AS [K4 - broj] " + UnitBase;

    private const string InvoiceBase =
        "FROM finance.Invoice i JOIN billing.InvoiceBatch b ON b.Id = i.InvoiceBatchId JOIN core.Company c ON c.Id = i.CompanyId " +
        "LEFT JOIN core.Partner cp ON cp.Id = c.PartnerId ";

    // ponytail: email = comm value containing '@', phone = first other value; add a channel code lookup if channels get richer.
    public static readonly IReadOnlyList<(string Name, string Title, bool NeedsPeriod, string Sql)> All =
    [
        ("EXPORT001", "Listing SZ - PD - vlasnik (aktivni)", false,
            UnitListing + $"WHERE c.Id IN {Ids} AND k.IsActive = 1 ORDER BY c.Id, t.IndexSort, u.SortingNumber"),
        ("EXPORT002", "Listing računa za period (YYMM)", true,
            "SELECT b.PeriodYYMM AS GrupaRacunaFXN, i.CompanyId AS ID_SK, cp.Name AS NazivSS, c.ShortName AS Zgrada, i.PartnerId AS ID_K, " +
            "i.PartnerName AS Kupac, i.SequenceNumber AS RBR, i.InvoiceTotal AS UkupnoRacun, i.InterestAmount AS KamataIznos, i.Total AS Ukupno, " +
            "i.PaymentReference AS PozivNaBroj " + InvoiceBase +
            $"WHERE c.Id IN {Ids} AND b.PeriodYYMM = @yymm AND i.IsCancelled = 0 ORDER BY i.CompanyId, i.PartnerId"),
        ("EXPORT003", "Sume stavki računa za period (YYMM)", true,
            "SELECT b.PeriodYYMM AS GrupaRacunaFXN, b.Caption AS GrupaRacunaFXT, c.ShortName AS Zgrada, l.Name AS Naziv, SUM(l.Quantity) AS SumOfKolicina, " +
            "l.PriceEur AS CenaE, l.ExchangeRateNbs AS NBS, l.PricePcs AS Iznos, SUM(l.TotalAmount) AS SumOfUkupnoRSD, l.SortIndex AS Sort, sp.Name AS Dobavljac " +
            "FROM finance.InvoiceLine l JOIN finance.Invoice i ON i.Id = l.InvoiceId JOIN billing.InvoiceBatch b ON b.Id = i.InvoiceBatchId " +
            "JOIN core.Company c ON c.Id = i.CompanyId LEFT JOIN billing.SupplierInvoice si ON si.Id = l.SupplierInvoiceId " +
            "LEFT JOIN core.PartnerAccount spa ON spa.Id = si.SupplierPartnerAccountId LEFT JOIN core.Partner sp ON sp.Id = spa.PartnerId " +
            $"WHERE c.Id IN {Ids} AND b.PeriodYYMM = @yymm " +
            "GROUP BY b.PeriodYYMM, b.Caption, c.ShortName, l.Name, l.PriceEur, l.ExchangeRateNbs, l.PricePcs, l.SortIndex, sp.Name, l.CompanyId " +
            "ORDER BY l.CompanyId, l.SortIndex"),
        ("EXPORT005", "Benefiti - svi", false,
            "SELECT bn.PeriodYYMM AS MesecYYMM, c.Id AS ID_SZ, c.ShortName AS Zgrada, t.Caption AS TipObj, u.Id AS Unit, pa.AccountNumber AS lnk_ID_K, " +
            "u.K1, u.K2, u.K3, p.Name AS Naziv, CASE WHEN bn.InvoiceId IS NULL THEN 0 ELSE -1 END AS Used " +
            "FROM billing.Benefit bn JOIN core.Contract k ON k.Id = bn.ContractId JOIN core.Unit u ON u.Id = k.UnitId JOIN core.Company c ON c.Id = u.CompanyId " +
            "LEFT JOIN core.PartnerAccount pa ON pa.ContractId = k.Id AND pa.Account = '2040' LEFT JOIN core.Partner p ON p.Id = pa.PartnerId " +
            $"LEFT JOIN core.ShortList t ON t.Id = u.UnitTypeId WHERE c.Id IN {Ids} ORDER BY bn.PeriodYYMM, c.Id, u.SortingNumber"),
        ("EXPORT006", "Suma računa po periodu i zgradi", false,
            "SELECT b.PeriodYYMM AS GrupaRacunaFXN, c.ShortName AS Zgrada, COUNT(i.Id) AS CountOfIDRacun " + InvoiceBase +
            $"WHERE c.Id IN {Ids} AND i.IsCancelled = 0 GROUP BY b.PeriodYYMM, c.ShortName ORDER BY b.PeriodYYMM, c.ShortName"),
        ("EXPORT007", "Listing SZ - PD - vlasnik (neaktivni)", false,
            UnitListing + $"WHERE c.Id IN {Ids} AND ISNULL(k.IsActive, 0) = 0 ORDER BY c.Id, t.IndexSort, u.SortingNumber"),
        ("EXPORT008", "Kontakt lista", false,
            "SELECT pa.AccountNumber AS ID_K, c.ShortName AS Zgrada, p.Name AS Naziv, " +
            "(SELECT TOP 1 x.ValueNormalized FROM core.PartnerComms x WHERE x.PartnerId = p.Id AND x.IsActive = 1 AND x.ValueNormalized NOT LIKE '%@%' ORDER BY x.SortIndex) AS Telefon, " +
            "u.Name AS naziv, m.ValueNormalized AS eMail, CASE WHEN m.IsRegisteredForInvoiceReceipt = 1 THEN -1 ELSE 0 END AS SendMailRacun, t.Caption AS TipObj " +
            UnitBase + "LEFT JOIN core.PartnerComms m ON m.PartnerId = p.Id AND m.IsActive = 1 AND m.ValueNormalized LIKE '%@%' " +
            $"WHERE c.Id IN {Ids} AND p.Id IS NOT NULL AND k.IsActive = 1 ORDER BY c.Id, u.SortingNumber"),
        ("SPISAK-STANARA-EXPORT", "Spisak stanara", false,
            "SELECT u.CompanyId AS lnkSkupstinaID, p.Name AS Naziv, t.Caption AS TipObj, u.SortingNumber AS BrojPD, u.Id AS SifraPD, u.Name AS naziv " +
            UnitBase + $"WHERE c.Id IN {Ids} ORDER BY u.CompanyId, u.SortingNumber"),
    ];

    public static IEndpointRouteBuilder MapLegacyFormatExports(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/companies/{companyId:int}/export/legacy", (int companyId) =>
                Results.Ok(All.Select(x => new LegacyExportResponse(x.Name, x.Title, x.NeedsPeriod))))
            .WithTags("Export").RequireAuthorization(SecurityConstants.CompanyAdminPolicy);
        endpoints.MapGet("/api/v1/companies/{companyId:int}/export/legacy/{name}",
                (int companyId, string name, int? yymm, string? encoding, HttpContext http, SzAppDbContext db, TimeProvider clock, CancellationToken ct) =>
                    RunAsync(http, db, clock, [companyId], name, yymm, encoding, ct))
            .WithTags("Export").RequireAuthorization(SecurityConstants.CompanyAdminPolicy);
        endpoints.MapGet("/api/v1/export/legacy/{name}",
                async (string name, int? yymm, string? encoding, HttpContext http, SzAppDbContext db, TimeProvider clock, CancellationToken ct) =>
                    await RunAsync(http, db, clock, await db.Companies.Select(x => x.Id).ToArrayAsync(ct), name, yymm, encoding, ct))
            .WithTags("Export").RequireAuthorization(policy => policy.RequireRole(SecurityConstants.RootRole));
        return endpoints;
    }

    private static async Task<IResult> RunAsync(HttpContext http, SzAppDbContext db, TimeProvider clock, int[] ids,
        string name, int? yymm, string? encoding, CancellationToken ct)
    {
        var export = All.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (export.Sql is null) return Results.NotFound();
        if (export.NeedsPeriod && yymm is null or < 1000 or > 9912)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["yymm"] = ["Period je obavezan (YYMM, npr. 2602)."] });
        System.Text.Encoding textEncoding;
        try { textEncoding = LegacyCsvWriter.ResolveEncoding(encoding); }
        catch (ArgumentException exception) { return Results.ValidationProblem(new Dictionary<string, string[]> { ["encoding"] = [exception.Message] }); }

        db.AuditLogs.Add(new AuditLog
        {
            CompanyId = ids.Length == 1 ? ids[0] : null,
            StaffId = int.TryParse(http.User.FindFirstValue(ClaimTypes.NameIdentifier), out var staffId) ? staffId : null,
            Timestamp = clock.GetUtcNow(), CorrelationId = http.TraceIdentifier, EntityType = "DataExport", Action = "LegacyExport",
            EventSource = "api", DetailsJson = JsonSerializer.Serialize(new { name = export.Name, companyIds = ids, yymm }),
        });
        await db.SaveChangesAsync(ct);

        var connection = db.Database.GetDbConnection();
        await db.Database.OpenConnectionAsync(ct);
        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = export.Sql;
            Add(command, "@ids", JsonSerializer.Serialize(ids));
            Add(command, "@yymm", yymm ?? 0);
            await using var reader = await command.ExecuteReaderAsync(ct);
            using var buffer = new MemoryStream();
            await using (var text = new StreamWriter(buffer, textEncoding, 65536, leaveOpen: true))
            {
                var csv = new LegacyCsvWriter(text);
                await csv.WriteRowAsync(Enumerable.Range(0, reader.FieldCount).Select(i => (object?)reader.GetName(i)), ct);
                var values = new object[reader.FieldCount];
                while (await reader.ReadAsync(ct))
                {
                    reader.GetValues(values);
                    await csv.WriteRowAsync(values.Select(v => v is DBNull ? null : v), ct);
                }
            }
            var file = $"{export.Name}-{(ids.Length == 1 ? ids[0].ToString(CultureInfo.InvariantCulture) : "all")}{(yymm is { } p ? $"-{p}" : "")}.csv";
            return Results.File(buffer.ToArray(), "text/csv", file);
        }
        finally { await db.Database.CloseConnectionAsync(); }
    }

    private static void Add(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
