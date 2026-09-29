using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SzApp.Domain.Billing;

public static class BillingCalculator
{
    public static BillingLineAmounts CalculateLine(BillingLineInput input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input.Name);
        if (input.Quantity < 0m || input.UnitPrice < 0m)
        {
            throw new DomainRuleException("billing.negative-line-value", "Količina i cena ne mogu biti negativne.");
        }

        if (input.VatRate is < 0m or > 100m)
        {
            throw new DomainRuleException("billing.invalid-vat-rate", "Stopa PDV-a mora biti između 0 i 100.");
        }

        var coefficients = new[] { input.K1, input.K2, input.K3, input.K4, input.K5 };
        if (coefficients.Any(x => x < 0m))
        {
            throw new DomainRuleException("billing.negative-coefficient", "Koeficijenti ne mogu biti negativni.");
        }

        var quantity = FinanceRounding.Money(input.Quantity);
        var unitPrice = FinanceRounding.Calculation(input.UnitPrice);
        var coefficient = coefficients.Aggregate(1m, (current, value) =>
            FinanceRounding.Calculation(current * FinanceRounding.Calculation(value)));
        var net = FinanceRounding.Money(quantity * unitPrice * coefficient);
        var vat = FinanceRounding.Money(net * input.VatRate / 100m);
        return new BillingLineAmounts(quantity, unitPrice, net, vat, FinanceRounding.Money(net + vat));
    }

    public static BillingInvoiceAmounts CalculateInvoice(
        IEnumerable<BillingLineInput> lines,
        decimal interestAmount = 0m)
    {
        var calculated = lines.Select(CalculateLine).ToArray();
        if (calculated.Length == 0)
        {
            throw new DomainRuleException("billing.invoice-without-lines", "Račun mora imati najmanje jednu stavku.");
        }

        var net = FinanceRounding.Money(calculated.Sum(x => x.NetAmount));
        // FIN-18: no proportional benefit reduction here -- the invoice engine zeroes the
        // manager's lines and archives them (InvoiceGenerationService, BenefitArchive).
        var vat = FinanceRounding.Money(calculated.Sum(x => x.VatAmount));
        var taxable = net;
        var interest = FinanceRounding.Money(interestAmount);
        if (interest < 0m)
        {
            throw new DomainRuleException("billing.negative-interest", "Kamata ne može biti negativna.");
        }

        return new BillingInvoiceAmounts(
            net,
            taxable,
            vat,
            interest,
            FinanceRounding.Money(taxable + vat + interest));
    }

    public static decimal CalculateNoticeTotal(decimal debt, decimal additionalCosts)
    {
        if (debt < 0m || additionalCosts < 0m)
        {
            throw new DomainRuleException("billing.invalid-notice-amount", "Dug i dodatni troškovi ne mogu biti negativni.");
        }

        return FinanceRounding.Money(debt + additionalCosts);
    }

    public static string CreateDeterministicKey<T>(T value)
    {
        var json = JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
    }
}
