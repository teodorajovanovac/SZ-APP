namespace SzApp.Domain;

public sealed record InvoiceAmounts(
    decimal GrossBase,
    decimal BenefitAmount,
    decimal NetBase,
    decimal VatAmount,
    decimal InterestAmount,
    decimal Total);

public static class InvoiceCalculator
{
    public static InvoiceAmounts Calculate(
        decimal grossBase,
        decimal vatRate,
        decimal benefitAmount = 0m,
        decimal interestAmount = 0m)
    {
        if (grossBase < 0m)
        {
            throw new DomainRuleException("invoice.negative-base", "Osnovica računa ne može biti negativna.");
        }

        if (vatRate is < 0m or > 100m)
        {
            throw new DomainRuleException("invoice.invalid-vat-rate", "Stopa PDV-a mora biti između 0 i 100.");
        }

        if (benefitAmount < 0m || benefitAmount > grossBase)
        {
            throw new DomainRuleException("invoice.invalid-benefit", "Benefit mora biti između nule i osnovice.");
        }

        if (interestAmount < 0m)
        {
            throw new DomainRuleException("invoice.negative-interest", "Kamata ne može biti negativna.");
        }

        var roundedGrossBase = FinanceRounding.Money(grossBase);
        var roundedBenefit = FinanceRounding.Money(benefitAmount);
        var netBase = FinanceRounding.Money(roundedGrossBase - roundedBenefit);
        var vat = FinanceRounding.Money(netBase * vatRate / 100m);
        var interest = FinanceRounding.Money(interestAmount);
        var total = FinanceRounding.Money(netBase + vat + interest);

        return new InvoiceAmounts(roundedGrossBase, roundedBenefit, netBase, vat, interest, total);
    }

    public static decimal ConvertCurrency(decimal amount, decimal exchangeRate) =>
        FinanceRounding.Calculation(amount * exchangeRate);
}
