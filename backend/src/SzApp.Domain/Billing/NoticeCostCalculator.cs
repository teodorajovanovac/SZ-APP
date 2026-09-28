namespace SzApp.Domain.Billing;

/// <summary>
/// Notice type codes, stored as ShortList(TableName = NoticeType).IndexValue (data-model:
/// "Samostalna opomena, Notifikacija na računu"; legacy GrupaOpomena vrsta 0 = text on the invoice).
/// </summary>
public static class NoticeTypes
{
    public const string ShortListTable = "NoticeType";
    /// <summary>"Obaveštenje" — notification printed on the invoice; never carries a cost (P11).</summary>
    public const int Notification = 0;
    /// <summary>Standalone notice (opomena) — carries the threshold cost.</summary>
    public const int Standalone = 1;

    public static bool CarriesCost(int noticeTypeCode) => noticeTypeCode != Notification;
}

/// <summary>A NoticeAditionalCosts row. DateEnd null = open-ended; CompanyId null = all companies.</summary>
public sealed record NoticeCostRule(
    int Id,
    DateOnly DateStart,
    DateOnly? DateEnd,
    int? CompanyId,
    decimal LowerAmount,
    decimal LowerLimit,
    decimal UpperAmount);

/// <summary>The three thresholds as snapshotted onto a NoticeBatch.</summary>
public sealed record NoticeCostThresholds(decimal LowerAmount, decimal LowerLimit, decimal UpperAmount);

/// <summary>P11: debt &lt; LowerLimit → LowerAmount; debt ≥ LowerLimit → UpperAmount; notifications cost 0.</summary>
public static class NoticeCostCalculator
{
    public static decimal Cost(decimal debt, NoticeCostThresholds? thresholds, bool carriesCost)
    {
        if (!carriesCost || thresholds is null) return 0m;
        return FinanceRounding.Money(debt < thresholds.LowerLimit ? thresholds.LowerAmount : thresholds.UpperAmount);
    }

    /// <summary>The rule in force on <paramref name="date"/>: a company-specific row beats a global one.</summary>
    public static NoticeCostRule? Applicable(IEnumerable<NoticeCostRule> rules, int companyId, DateOnly date)
    {
        var active = rules.Where(x => x.DateStart <= date && (x.DateEnd is null || date <= x.DateEnd)).ToArray();
        return active.Where(x => x.CompanyId == companyId).OrderByDescending(x => x.DateStart).FirstOrDefault()
            ?? active.Where(x => x.CompanyId is null).OrderByDescending(x => x.DateStart).FirstOrDefault();
    }

    public static NoticeCostThresholds? Snapshot(NoticeCostRule? rule) =>
        rule is null ? null : new(rule.LowerAmount, rule.LowerLimit, rule.UpperAmount);

    /// <summary>Validates a new/edited rule against the others (its own Id is ignored). Same scope = same CompanyId (null = global).</summary>
    public static void EnsureValid(NoticeCostRule rule, IEnumerable<NoticeCostRule> existing)
    {
        if (rule.DateEnd is { } end && end < rule.DateStart)
            throw new DomainRuleException("notice-costs.invalid-period", "Datum početka mora biti pre datuma završetka.");
        if (rule.LowerAmount < 0m || rule.LowerLimit < 0m || rule.UpperAmount < 0m)
            throw new DomainRuleException("notice-costs.negative-amount", "Iznosi i granica ne mogu biti negativni.");
        var maxEnd = DateOnly.MaxValue;
        var overlapping = existing.Any(x => x.Id != rule.Id && x.CompanyId == rule.CompanyId
            && rule.DateStart <= (x.DateEnd ?? maxEnd) && x.DateStart <= (rule.DateEnd ?? maxEnd));
        if (overlapping)
            throw new DomainRuleException("notice-costs.overlap", "Za isti obim (kompanija ili globalno) već postoji red čiji se period preklapa.");
    }
}
