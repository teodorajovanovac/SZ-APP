namespace SzApp.Data.Entities;

public sealed class PartnerAddress
{
    public int Id { get; set; }
    public int PartnerId { get; set; }
    public int AddressId { get; set; }
    public int AddressTypeId { get; set; }
    public bool IsDefault { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Partner Partner { get; set; } = null!;
    public Address Address { get; set; } = null!;
    public ShortList AddressType { get; set; } = null!;
}

public sealed class PartnerCommunication
{
    public int Id { get; set; }
    public int PartnerId { get; set; }
    public int ChannelId { get; set; }
    public string ValueNormalized { get; set; } = string.Empty;
    public string? Note { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPrimary { get; set; }
    public int SortIndex { get; set; }
    public bool IsRegisteredForInvoiceReceipt { get; set; }
    public Partner Partner { get; set; } = null!;
    public ShortList Channel { get; set; } = null!;
}

public sealed class ExchangeRate
{
    public int Id { get; set; }
    public decimal Rate { get; set; }
    public DateOnly RateDateFrom { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class FiscalYear : ICompanyOwned
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int Year { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsArchived { get; set; }
    public string? Display { get; set; }
    public string? Folder { get; set; }
    public string? FileName { get; set; }
    public bool IsCurrent { get; set; }
    public Company Company { get; set; } = null!;
}

public sealed class DocumentCategory
{
    public int Id { get; set; }
    public string? GroupName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int? RetentionPeriodYears { get; set; }
}

public sealed class LocationCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public int SortIndex { get; set; }
    public LocationCategory? Parent { get; set; }
}
