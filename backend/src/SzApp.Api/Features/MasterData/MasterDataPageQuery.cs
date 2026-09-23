namespace SzApp.Api.Features.MasterData;

public sealed class MasterDataPageQuery
{
    private const int MaximumPageSize = 100;

    public int? Page { get; init; }
    public int? PageSize { get; init; }
    public string? Search { get; init; }
    public string? SortBy { get; init; }
    public bool? Descending { get; init; }

    public int NormalizedPage => Math.Max(1, Page ?? 1);
    public int NormalizedPageSize => Math.Clamp(PageSize ?? 25, 1, MaximumPageSize);
    public bool NormalizedDescending => Descending ?? false;
    public int Skip => (NormalizedPage - 1) * NormalizedPageSize;
    public string NormalizedSearch => Search?.Trim() ?? string.Empty;
}
