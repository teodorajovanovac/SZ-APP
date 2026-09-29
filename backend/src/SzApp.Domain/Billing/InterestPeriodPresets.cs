namespace SzApp.Domain.Billing;

public sealed record InterestPeriodPreset(string Key, DateOnly Start, DateOnly End);

/// <summary>
/// P10: the interest period is a per-run parameter. These are only suggestions; the run takes explicit dates.
/// Reference month = the month before the billing period (legacy bills interest for the previous month).
/// </summary>
public static class InterestPeriodPresets
{
    /// <summary>Legacy PrethodnaValuta → DatumStanja, both inclusive (audit 9.4).</summary>
    public const string FromPreviousDueDate = "fromPreviousDueDate";
    public const string WholeMonth = "wholeMonth";
    /// <summary>Previous due date → day before this batch's due date (so the next run can start on it without overlap).</summary>
    public const string DueToDue = "dueToDue";
    public const string Custom = "custom";

    public static readonly IReadOnlyList<string> Keys = [FromPreviousDueDate, WholeMonth, DueToDue, Custom];

    /// <summary>Company (fallback global) Setting holding the default preset key.</summary>
    public const string DefaultPresetSettingKey = "Interest.DefaultPeriodPreset";

    public static IReadOnlyList<InterestPeriodPreset> Suggest(
        int periodYYMM,
        DateOnly? previousValueDate,
        DateOnly? balanceAsOfDate,
        DateOnly? dueDate,
        DateOnly? previousBatchDueDate)
    {
        var month = periodYYMM % 100;
        if (periodYYMM < 1001 || periodYYMM > 9912 || month is < 1 or > 12)
            throw new DomainRuleException("interest.invalid-billing-period", "Period obračuna (YYMM) nije ispravan.");
        var monthStart = new DateOnly(2000 + periodYYMM / 100, month, 1).AddMonths(-1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        var previousDue = previousValueDate ?? previousBatchDueDate ?? monthStart;
        var stateDate = balanceAsOfDate ?? monthEnd;
        var dueEnd = (dueDate ?? previousDue.AddMonths(1)).AddDays(-1);

        return
        [
            new(FromPreviousDueDate, previousDue, Max(previousDue, stateDate)),
            new(WholeMonth, monthStart, monthEnd),
            new(DueToDue, previousDue, Max(previousDue, dueEnd)),
            new(Custom, previousDue, Max(previousDue, stateDate)),
        ];
    }

    public static string NormalizeKey(string? key) =>
        key is not null && Keys.Contains(key.Trim()) ? key.Trim() : FromPreviousDueDate;

    public static bool Overlaps(DateOnly start, DateOnly end, DateOnly otherStart, DateOnly otherEnd) =>
        start <= otherEnd && otherStart <= end;

    private static DateOnly Max(DateOnly a, DateOnly b) => a > b ? a : b;
}
