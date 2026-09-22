using SzApp.Domain.Platform;

namespace SzApp.Data.Entities;

public sealed class DocumentRecord : ICompanyOwned
{
    public long Id { get; set; }
    public int CompanyId { get; set; }
    public int OwnerStaffId { get; set; }
    public int? DocumentTypeId { get; set; }
    public string? SourceTable { get; set; }
    public int? ReferenceId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Sha256 { get; set; } = string.Empty;
    public DateOnly? DocumentDate { get; set; }
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public Company Company { get; set; } = null!;
    public ApplicationUser OwnerStaff { get; set; } = null!;
}

public enum EmailSendStatus { Draft = 1, Queued = 2, Sending = 3, Sent = 4, Failed = 5 }

public sealed class SentEmail : ICompanyOwned
{
    public long Id { get; set; }
    public int CompanyId { get; set; }
    public int CreatedByStaffId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string ToAddress { get; set; } = string.Empty;
    public string? Cc { get; set; }
    public string? Bcc { get; set; }
    public string BodyHtml { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public bool IsArchived { get; set; }
    public EmailSendStatus Status { get; set; }
    public string? SendDescription { get; set; }
    public int AttemptCount { get; set; }
    public DateTimeOffset? NextAttemptAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public Company Company { get; set; } = null!;
    public ApplicationUser CreatedByStaff { get; set; } = null!;
    public ICollection<SentEmailAttachment> Attachments { get; } = new List<SentEmailAttachment>();
}

public sealed class SentEmailAttachment
{
    public long Id { get; set; }
    public long SentEmailId { get; set; }
    public long DocumentId { get; set; }
    public SentEmail SentEmail { get; set; } = null!;
    public DocumentRecord Document { get; set; } = null!;
}

public sealed class Setting
{
    public long Id { get; set; }
    public int? CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? ValueMax { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public Company? Company { get; set; }
}

public sealed class PlatformEvent : ICompanyOwned
{
    public long Id { get; set; }
    public int CompanyId { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
    public DateTimeOffset? DecidedAt { get; set; }
    public DateTimeOffset? ExecutedAt { get; set; }
    public int RequestedByStaffId { get; set; }
    public int? DecidedByStaffId { get; set; }
    public int? ExecutedByStaffId { get; set; }
    public int? ContractId { get; set; }
    public int? PartnerId { get; set; }
    public int? UnitId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? PreviousValue { get; set; }
    public string? NewValue { get; set; }
    public string? FieldsRelated { get; set; }
    public int? RequestTypeId { get; set; }
    public string? RequestBy { get; set; }
    public string? RequestThrough { get; set; }
    public PlatformEventStatus Status { get; set; }
    public string? DecisionReason { get; set; }
    public bool EmergencyOverride { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

public sealed class SelectionBasket : ICompanyOwned
{
    public Guid Id { get; set; }
    public int CompanyId { get; set; }
    public int OwnerStaffId { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public ApplicationUser OwnerStaff { get; set; } = null!;
}

public sealed class ImportDefinition : ICompanyOwned
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string FileMask { get; set; } = string.Empty;
    public int? ImportSourceId { get; set; }
    public string? FilePath { get; set; }
    public string TargetHeaderTable { get; set; } = string.Empty;
    public string? TargetLineTable { get; set; }
    public bool IsActive { get; set; }
    public int SortIndex { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ICollection<ImportMappingGroup> Groups { get; } = new List<ImportMappingGroup>();
}

public sealed class ImportMappingGroup
{
    public int Id { get; set; }
    public int ImportDefinitionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TargetTable { get; set; } = string.Empty;
    public string SourcePath { get; set; } = string.Empty;
    public bool IsRepeating { get; set; }
    public ImportDefinition Definition { get; set; } = null!;
    public ICollection<ImportMapping> Mappings { get; } = new List<ImportMapping>();
}

public sealed class ImportMapping
{
    public int Id { get; set; }
    public int ImportMappingGroupId { get; set; }
    public string TargetField { get; set; } = string.Empty;
    public string SourcePath { get; set; } = string.Empty;
    public string? SourceNode { get; set; }
    public int? MappingTypeId { get; set; }
    public int? DataTypeId { get; set; }
    public string? DefaultValue { get; set; }
    public bool IsRequired { get; set; }
    public bool IsKey { get; set; }
    public bool IsLookup { get; set; }
    public string? LookupTable { get; set; }
    public string? LookupField { get; set; }
    public string? LookupValueField { get; set; }
    public string? Format { get; set; }
    public int SortIndex { get; set; }
    public string? Description { get; set; }
    public ImportMappingGroup Group { get; set; } = null!;
}

public sealed class Language
{
    public string Code { get; set; } = string.Empty;
    public string? Name { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public int? SortIndex { get; set; }
}

public sealed class Translation
{
    public long Id { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string ResourceKey { get; set; } = string.Empty;
    public int? ResourceId { get; set; }
    public int? CompanyId { get; set; }
    public string Value { get; set; } = string.Empty;
    public Language Language { get; set; } = null!;
    public Company? Company { get; set; }
}
