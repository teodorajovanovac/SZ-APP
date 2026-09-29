namespace SzApp.Domain.Billing;

/// <summary>
/// R0 input: one unit's coefficients (audit 9.1). CustomerId is the unit's billable partner
/// (Contract.InvoicePartnerId ?? OwnerPartnerId ?? TenantPartnerId) at generation time.
/// IsActive = the unit's Contract.IsActive (P3: numerators only count active units, denominators
/// -- SUM-* company/unit-type totals -- include inactive ones too).
/// </summary>
public sealed record R0UnitRow(int CustomerId, int UnitTypeId, bool IsActive, decimal K1, decimal K2, decimal K3, decimal K4, decimal K5);

/// <summary>R0 input: one eligible supplier invoice (already filtered: Cat1=1/Planned, TipObracuna != 99, marker match).</summary>
public sealed record R0SupplierInput(
    int SupplierInvoiceId,
    int SupplierPartnerAccountId,
    int CalculationTypeId,
    IReadOnlyList<int> UnitTypeIds,
    decimal InvoiceAmountRsd,
    decimal InvoiceAmountEur,
    decimal PerCoefficientAmountRsd,
    decimal PerCoefficientAmountEur,
    decimal VatRatePercent);

public sealed record R1CustomerLine(int CustomerId, int SupplierInvoiceId, int SupplierPartnerAccountId, int UnitTypeId, R1LineResult Result);

/// <summary>
/// R0 aggregation + R1 calculation (audit 9.1). Pure function over already-loaded rows so it can
/// be unit tested without a database. R2 (invoice header/PrethodniDug) and R3 (numbering) happen
/// in the EF-backed orchestration (InvoiceGenerationService) since they need live GL balances.
/// </summary>
public static class InvoiceGenerationEngine
{
    public static IReadOnlyList<R1CustomerLine> GenerateLines(
        IReadOnlyList<R0UnitRow> units,
        IReadOnlyList<R0SupplierInput> supplierInvoices,
        decimal nbsRate)
    {
        var result = new List<R1CustomerLine>();
        foreach (var invoice in supplierInvoices)
        {
            foreach (var unitTypeId in invoice.UnitTypeIds.Distinct())
            {
                var allOfType = units.Where(u => u.UnitTypeId == unitTypeId).ToArray();
                if (allOfType.Length == 0) continue;

                // Denominators: company-wide, this unit type, active AND inactive units (P3).
                var sumK1K2 = allOfType.Sum(u => u.K1 * u.K2);
                var sumK2 = allOfType.Sum(u => u.K2);

                // Numerators: only active units, grouped per customer.
                foreach (var customer in allOfType.Where(u => u.IsActive).GroupBy(u => u.CustomerId))
                {
                    var row = new R0Aggregate(
                        InvoiceAmountRsd: invoice.InvoiceAmountRsd,
                        InvoiceAmountEur: invoice.InvoiceAmountEur,
                        PerCoefficientAmountRsd: invoice.PerCoefficientAmountRsd,
                        PerCoefficientAmountEur: invoice.PerCoefficientAmountEur,
                        SumK1K2: sumK1K2,
                        SumK2: sumK2,
                        K1: customer.Sum(u => u.K1),
                        K2: customer.Sum(u => u.K2),
                        K3: customer.Sum(u => u.K3),
                        K2xK3: customer.Sum(u => u.K2 * u.K3),
                        K2xK4: customer.Sum(u => u.K2 * u.K4),
                        K2xK5: customer.Sum(u => u.K2 * u.K5),
                        SumK1: customer.Sum(u => u.K1),
                        SumK4: customer.Sum(u => u.K4));

                    var r1 = InvoiceCalculationTypes.Calculate(invoice.CalculationTypeId, row, invoice.VatRatePercent, nbsRate);
                    if (r1 is null) continue; // type 99 or zero quantity -- nothing to bill (item 2)

                    result.Add(new R1CustomerLine(customer.Key, invoice.SupplierInvoiceId, invoice.SupplierPartnerAccountId, unitTypeId, r1));
                }
            }
        }
        return result;
    }
}
