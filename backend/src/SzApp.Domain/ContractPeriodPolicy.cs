namespace SzApp.Domain;

public static class ContractPeriodPolicy
{
    public static bool Overlaps(DateOnly start, DateOnly? end, DateOnly otherStart, DateOnly? otherEnd)
    {
        var effectiveEnd = end ?? DateOnly.MaxValue;
        var effectiveOtherEnd = otherEnd ?? DateOnly.MaxValue;
        return start <= effectiveOtherEnd && otherStart <= effectiveEnd;
    }
}
