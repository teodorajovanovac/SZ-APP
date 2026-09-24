namespace SzApp.Contracts.MasterData;

public sealed record PagedResponse<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record CompanyDetailResponse(
    int Id,
    int PartnerId,
    int? ManagerId,
    string ShortName,
    string PrintName,
    string? RelativeFolderName,
    int? CompanyTypeId,
    int? VatTypeId,
    DateOnly? LedgerEntryDate,
    int? LocationCategoryId,
    string? Note,
    int? SortIndex,
    string? ExternalAccount,
    string RowVersion);

public sealed record CreateCompanyRequest(
    int PartnerId,
    int? ManagerId,
    string ShortName,
    string PrintName,
    string? RelativeFolderName,
    int? CompanyTypeId,
    int? VatTypeId,
    DateOnly? LedgerEntryDate,
    int? LocationCategoryId,
    string? Note,
    int? SortIndex,
    string? ExternalAccount);

public sealed record UpdateCompanyRequest(
    int PartnerId,
    int? ManagerId,
    string ShortName,
    string PrintName,
    string? RelativeFolderName,
    int? CompanyTypeId,
    int? VatTypeId,
    DateOnly? LedgerEntryDate,
    int? LocationCategoryId,
    string? Note,
    int? SortIndex,
    string? ExternalAccount,
    string RowVersion);

public sealed record LocationCategoryResponse(int Id, string Name, int? ParentId, int SortIndex);

public sealed record PartnerResponse(
    int Id,
    int CompanyId,
    string ShortName,
    string Name,
    string? RegistrationNumber,
    string? TaxNumber,
    string? Jbkjs,
    string? MaskedIdCardNumber,
    string? MaskedJmbg,
    int? PartnerTypeId,
    string Language,
    string? Note);

public sealed record SavePartnerRequest(
    string ShortName,
    string Name,
    string? RegistrationNumber,
    string? TaxNumber,
    string? Jbkjs,
    string? IdCardNumber,
    string? Jmbg,
    int? PartnerTypeId,
    string Language,
    string? Note);

public sealed record AddressResponse(
    int Id,
    string StreetAddress,
    string? PostalCode,
    string City,
    string CountryCode);

public sealed record SaveAddressRequest(
    string StreetAddress,
    string? PostalCode,
    string City,
    string CountryCode);

public sealed record StaffAccessResponse(
    int Id,
    int StaffId,
    string StaffEmail,
    int CompanyId,
    string StaffRole);

public sealed record SaveStaffAccessRequest(int StaffId, string StaffRole);

// Contracts for the next model-integration step. Endpoints are intentionally not
// mapped until their entities and EF configurations exist in SzApp.Data.
public sealed record BuildingEntranceResponse(
    int Id,
    int CompanyId,
    string? BuildingName,
    string? EntranceName,
    int? AddressId,
    string? BuildingLabel,
    string? Description,
    int? SortIndex,
    string RowVersion);

public sealed record SaveBuildingEntranceRequest(
    string? BuildingName,
    string? EntranceName,
    int? AddressId,
    string? BuildingLabel,
    string? Description,
    int? SortIndex,
    string? RowVersion);

public sealed record UnitResponse(
    int Id,
    int CompanyId,
    string? Name,
    int? ContractId,
    int? UnitTypeId,
    int? BuildingEntranceId,
    string? Note,
    int? SortingNumber,
    decimal? K1,
    decimal? K2,
    decimal? K3,
    decimal? K4,
    decimal? K5,
    int? FloorNumber,
    string RowVersion);

public sealed record SaveUnitRequest(
    string? Name,
    int? UnitTypeId,
    int? BuildingEntranceId,
    string? Note,
    int? SortingNumber,
    decimal? K1,
    decimal? K2,
    decimal? K3,
    decimal? K4,
    decimal? K5,
    int? FloorNumber,
    string? RowVersion);

public sealed record ContractResponse(
    int Id,
    int UnitId,
    int? AccountNumber,
    int? OwnerPartnerId,
    int? InvoicePartnerId,
    int? TenantPartnerId,
    DateOnly ContractDate,
    DateOnly? ContractEndDate,
    DateOnly? InvoiceStartDate,
    DateOnly? InvoiceEndDate,
    bool IsActive,
    string? Note,
    string? InvoiceDeliveryLocation,
    int? InvoiceDeliveryUnitId,
    bool IsPrintInvoiceMandatory,
    bool IsPrintInvoiceToPostOffice,
    bool IsPrintInvoiceSkipped,
    string? ExportExternalAccount,
    string RowVersion);

public sealed record ReplaceContractRequest(
    int? AccountNumber,
    int? OwnerPartnerId,
    int? InvoicePartnerId,
    int? TenantPartnerId,
    DateOnly EffectiveFrom,
    DateOnly? InvoiceStartDate,
    DateOnly? InvoiceEndDate,
    string? Note,
    string? InvoiceDeliveryLocation,
    int? InvoiceDeliveryUnitId,
    bool IsPrintInvoiceMandatory,
    bool IsPrintInvoiceToPostOffice,
    bool IsPrintInvoiceSkipped,
    string? ExportExternalAccount,
    string? CurrentContractRowVersion);

public sealed record PartnerAccountResponse(
    int Id,
    int? CompanyId,
    string Account,
    int PartnerId,
    int? ContractId,
    int AccountNumber,
    string RowVersion);

public sealed record SavePartnerAccountRequest(
    string Account,
    int PartnerId,
    int? ContractId,
    int AccountNumber,
    string? RowVersion);

public sealed record BankAccountResponse(
    int Id,
    int? CompanyId,
    string? AccountNumber,
    bool IsActive,
    int? PartnerId,
    int? SortIndex,
    string Currency,
    string RowVersion);

public sealed record SaveBankAccountRequest(
    string? AccountNumber,
    bool IsActive,
    int? PartnerId,
    int? SortIndex,
    string Currency,
    string? RowVersion);
