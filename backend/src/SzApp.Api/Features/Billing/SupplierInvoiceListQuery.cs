namespace SzApp.Api.Features.Billing;

// Same nullable-properties style as MasterDataPageQuery: everything optional so
// [AsParameters] binding never treats a query param as required.
public sealed class SupplierInvoiceListQuery
{
    public int? Page { get; init; }
    public int? PageSize { get; init; }
    public int? PeriodYYMM { get; init; }
    public int? SupplierPartnerAccountId { get; init; }
    public bool? HasExtraordinaryMarker { get; init; }
    public int? DocumentTypeId { get; init; }
}
