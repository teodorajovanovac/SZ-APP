namespace SzApp.Contracts.MasterData;

/// <summary>
/// One row of the cross-company "Ugovori" (Contracts) search/browse screen.
/// Carries both the id and the display name for every clickable column so the
/// frontend can build drill-through links without a second round-trip.
/// </summary>
public sealed record ContractOverviewRowResponse(
    int CompanyId,
    string CompanyShortName,
    int? PartnerAccountId,
    int? PartnerAccountNumber,
    int? BuildingEntranceId,
    string? BuildingEntranceName,
    int? PartnerId,
    string? PartnerName,
    int UnitId,
    string? UnitName,
    string? UnitTypeName,
    int ContractId,
    bool IsActive);
