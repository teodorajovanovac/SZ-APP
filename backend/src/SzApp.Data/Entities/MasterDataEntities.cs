namespace SzApp.Data.Entities;

public interface ICompanyOwned
{
    int CompanyId { get; }
}

public sealed class Company
{
    public int Id { get; set; }
    public int PartnerId { get; set; }
    public int? ManagerId { get; set; }
    public string ShortName { get; set; } = string.Empty;
    public string PrintName { get; set; } = string.Empty;
    public string? RelativeFolderName { get; set; }
    public int? CompanyTypeId { get; set; }
    public int? VatTypeId { get; set; }
    public DateOnly? LedgerEntryDate { get; set; }
    public int? LocationCategoryId { get; set; }
    public string? Note { get; set; }
    public int? SortIndex { get; set; }
    public string? ExternalAccount { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public Partner Partner { get; set; } = null!;
    public Partner? Manager { get; set; }
    public ShortList? CompanyType { get; set; }
    public ShortList? VatType { get; set; }
    public LocationCategory? LocationCategory { get; set; }
    public ICollection<StaffAccess> StaffAccess { get; } = new List<StaffAccess>();
}

public sealed class Partner
{
    public int Id { get; set; }
    public int? CompanyId { get; set; }
    public string ShortName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? RegistrationNumber { get; set; }
    public string? TaxNumber { get; set; }
    public string? Jbkjs { get; set; }
    public string? IdCardNumber { get; set; }
    public string? Jmbg { get; set; }
    public int? PartnerTypeId { get; set; }
    public string Language { get; set; } = "sr-Latn";
    public string? Note { get; set; }
    public Company? OwningCompany { get; set; }
    public ShortList? PartnerType { get; set; }
}

public sealed class Address
{
    public int Id { get; set; }
    public string StreetAddress { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string City { get; set; } = string.Empty;
    public string CountryCode { get; set; } = "RS";
}

public sealed class ShortList
{
    public int Id { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? Description { get; set; }
    public int IndexValue { get; set; }
    public int IndexSort { get; set; }
    public string? IndexKey { get; set; }
}
