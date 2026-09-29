namespace SzApp.Domain.Billing;

/// <summary>
/// R1 calculation-type whitelist from audit 9.1 (legacy modRacun.GenerisanjeRacunaCodePoSK).
/// Deliberately a fixed table, not a generic parser: legacy's generic parser has bugs
/// (skips zero factors, doesn't reset a temp variable) that must not be reproduced.
/// Only IZNOS (amount) and KOLICINA (quantity) columns are meaningful per type.
/// </summary>
public enum InvoiceAmountSource
{
    /// <summary>Types 1/51: invoice amount distributed over SUM(K1*K2) across all (incl. inactive) units.</summary>
    InvoiceAmountOverK1K2,

    /// <summary>Types 2/52: invoice amount distributed over SUM(K2) across all (incl. inactive) units.</summary>
    InvoiceAmountOverK2,

    /// <summary>Types 9-14/59-65: the supplier invoice line's own per-coefficient amount, used directly.</summary>
    PerCoefficientAmount,

    /// <summary>Type 99: never invoiced.</summary>
    NotInvoiced
}

public enum InvoiceQuantitySource
{
    K1,
    K2,
    K2xK3,
    K2xK4,
    K2xK5,
    /// <summary>Types 14/64: SumK1 * SumK4 (product of the two aggregate sums). Owner question P6:
    /// could also mean sum-of-products; not yet confirmed — see audit 9.11 P6.</summary>
    SumK1TimesSumK4,
    K3
}

public sealed record InvoiceCalculationTypeDefinition(
    int Id,
    bool IsEur,
    InvoiceAmountSource AmountSource,
    InvoiceQuantitySource? QuantitySource);

/// <summary>
/// Inputs for one R0 aggregate row (customer x unit type x supplier invoice), already summed
/// per audit 9.1 R0: SumOfK1..K5, K1xK2, K2xK3, K2xK4, K2xK5. Denominators (SumK1K2, SumK2)
/// include inactive units (P3 confirmed).
/// </summary>
public sealed record R0Aggregate(
    decimal InvoiceAmountRsd,
    decimal InvoiceAmountEur,
    decimal PerCoefficientAmountRsd,
    decimal PerCoefficientAmountEur,
    decimal SumK1K2,
    decimal SumK2,
    decimal K1,
    decimal K2,
    decimal K3,
    decimal K2xK3,
    decimal K2xK4,
    decimal K2xK5,
    decimal SumK1,
    decimal SumK4);

public sealed record R1LineResult(decimal Quantity, decimal UnitAmountRsd, decimal NetAmount, decimal VatAmount, decimal TotalAmount);

public static class InvoiceCalculationTypes
{
    // Seed table, verbatim from audit 9.1 (18 rows). In production use: 9, 60, 65, 62, 59.
    public static readonly IReadOnlyDictionary<int, InvoiceCalculationTypeDefinition> ById =
        new List<InvoiceCalculationTypeDefinition>
        {
            new(1, false, InvoiceAmountSource.InvoiceAmountOverK1K2, InvoiceQuantitySource.K1),
            new(2, false, InvoiceAmountSource.InvoiceAmountOverK2, InvoiceQuantitySource.K2),
            new(9, false, InvoiceAmountSource.PerCoefficientAmount, InvoiceQuantitySource.K1),
            new(10, false, InvoiceAmountSource.PerCoefficientAmount, InvoiceQuantitySource.K2),
            new(11, false, InvoiceAmountSource.PerCoefficientAmount, InvoiceQuantitySource.K2xK3),
            new(12, false, InvoiceAmountSource.PerCoefficientAmount, InvoiceQuantitySource.K2xK4),
            new(13, false, InvoiceAmountSource.PerCoefficientAmount, InvoiceQuantitySource.K2xK5),
            new(14, false, InvoiceAmountSource.PerCoefficientAmount, InvoiceQuantitySource.SumK1TimesSumK4),
            new(99, false, InvoiceAmountSource.NotInvoiced, null),
            new(51, true, InvoiceAmountSource.InvoiceAmountOverK1K2, InvoiceQuantitySource.K1),
            new(52, true, InvoiceAmountSource.InvoiceAmountOverK2, InvoiceQuantitySource.K2),
            new(59, true, InvoiceAmountSource.PerCoefficientAmount, InvoiceQuantitySource.K1),
            new(60, true, InvoiceAmountSource.PerCoefficientAmount, InvoiceQuantitySource.K2),
            new(61, true, InvoiceAmountSource.PerCoefficientAmount, InvoiceQuantitySource.K2xK3),
            new(62, true, InvoiceAmountSource.PerCoefficientAmount, InvoiceQuantitySource.K2xK4),
            new(63, true, InvoiceAmountSource.PerCoefficientAmount, InvoiceQuantitySource.K2xK5),
            new(64, true, InvoiceAmountSource.PerCoefficientAmount, InvoiceQuantitySource.SumK1TimesSumK4),
            new(65, true, InvoiceAmountSource.PerCoefficientAmount, InvoiceQuantitySource.K3),
        }.ToDictionary(x => x.Id);

    /// <summary>
    /// R1: computes one invoice line's Suma/PDV/UkupnoRSD for a calculation type.
    /// EUR types: CenaE = amount, Iznos = CenaE x NBS with NO intermediate rounding.
    /// Suma = Round(Kol x Iznos, 2); PDV = Round(Suma x rate/100, 2); UkupnoRSD = Suma + PDV.
    /// Returns null for type 99 (not invoiced) or when quantity resolves to zero (nothing to bill).
    /// </summary>
    public static R1LineResult? Calculate(int calculationTypeId, R0Aggregate row, decimal vatRatePercent, decimal nbsRate)
    {
        if (!ById.TryGetValue(calculationTypeId, out var type))
        {
            throw new DomainRuleException("billing.unknown-calculation-type", $"Nepoznat tip obračuna {calculationTypeId}.");
        }

        if (type.AmountSource == InvoiceAmountSource.NotInvoiced)
        {
            return null;
        }

        var amount = type.AmountSource switch
        {
            InvoiceAmountSource.InvoiceAmountOverK1K2 =>
                row.SumK1K2 == 0m ? 0m : (type.IsEur ? row.InvoiceAmountEur : row.InvoiceAmountRsd) / row.SumK1K2,
            InvoiceAmountSource.InvoiceAmountOverK2 =>
                row.SumK2 == 0m ? 0m : (type.IsEur ? row.InvoiceAmountEur : row.InvoiceAmountRsd) / row.SumK2,
            InvoiceAmountSource.PerCoefficientAmount =>
                type.IsEur ? row.PerCoefficientAmountEur : row.PerCoefficientAmountRsd,
            _ => 0m
        };

        var quantity = type.QuantitySource switch
        {
            InvoiceQuantitySource.K1 => row.K1,
            InvoiceQuantitySource.K2 => row.K2,
            InvoiceQuantitySource.K3 => row.K3,
            InvoiceQuantitySource.K2xK3 => row.K2xK3,
            InvoiceQuantitySource.K2xK4 => row.K2xK4,
            InvoiceQuantitySource.K2xK5 => row.K2xK5,
            InvoiceQuantitySource.SumK1TimesSumK4 => row.SumK1 * row.SumK4,
            _ => 0m
        };

        // EUR: convert with NO intermediate rounding (audit 9.1 R1). RSD: amount is already in RSD.
        var iznos = type.IsEur ? amount * nbsRate : amount;

        var suma = FinanceRounding.Money(quantity * iznos);
        var pdv = FinanceRounding.Money(suma * vatRatePercent / 100m);
        var ukupno = suma + pdv;

        return new R1LineResult(quantity, amount, suma, pdv, ukupno);
    }
}
