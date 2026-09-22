namespace SzApp.Domain;

public static class FinanceRounding
{
    public const int MoneyScale = 2;
    public const int CalculationScale = 4;

    public static decimal Money(decimal value) =>
        decimal.Round(value, MoneyScale, MidpointRounding.AwayFromZero);

    public static decimal Calculation(decimal value) =>
        decimal.Round(value, CalculationScale, MidpointRounding.AwayFromZero);
}
