namespace SzApp.Api.Features.MasterData;

public sealed class MasterDataPageQuery
{
    private const int MaximumPageSize = 100;

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 25;
    public string? Search { get; init; }
    public string? SortBy { get; init; }
    public bool Descending { get; init; }

    public int NormalizedPage => Math.Max(1, Page);
    public int NormalizedPageSize => Math.Clamp(PageSize, 1, MaximumPageSize);
    public int Skip => (NormalizedPage - 1) * NormalizedPageSize;
    public string NormalizedSearch => Search?.Trim() ?? string.Empty;
}
