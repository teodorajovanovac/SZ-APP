namespace SzApp.Contracts.Platform;

public sealed record DocumentResponse(long Id, int CompanyId, string FileName, string ContentType, long Size, string Sha256, DateTimeOffset CreatedAt, string? Description, string RowVersion);
public sealed record CreateDocumentMetadata(int? DocumentTypeId, string? SourceTable, int? ReferenceId, DateOnly? DocumentDate, string? Description, int? CategoryId);

public sealed record SentEmailResponse(long Id, string Subject, string ToAddress, string? Cc, string? Bcc, string BodyHtml, DateTimeOffset CreatedAt, DateTimeOffset? SentAt, string Status, string? SendDescription, int AttemptCount, IReadOnlyCollection<long> DocumentIds, string RowVersion);
public sealed record CreateSentEmailRequest(string Subject, string ToAddress, string? Cc, string? Bcc, string BodyHtml, IReadOnlyCollection<long>? DocumentIds);

public sealed record SettingResponse(long Id, int? CompanyId, string Name, string Key, string? Value, string? Description, string? Category, string? ValueMax, string RowVersion);
public sealed record SaveSettingRequest(string Name, string Key, string? Value, string? Description, string? Category, string? ValueMax, string? RowVersion);

public sealed record PlatformEventResponse(long Id, int CompanyId, DateTimeOffset RequestedAt, DateTimeOffset? DecidedAt, DateTimeOffset? ExecutedAt, int RequestedByStaffId, int? DecidedByStaffId, int? ExecutedByStaffId, int? ContractId, int? PartnerId, int? UnitId, string Description, string? PreviousValue, string? NewValue, string? FieldsRelated, int? RequestTypeId, string? RequestBy, string? RequestThrough, string Status, string? DecisionReason, bool EmergencyOverride, string RowVersion);
public sealed record CreatePlatformEventRequest(int? ContractId, int? PartnerId, int? UnitId, string Description, string? PreviousValue, string? NewValue, string? FieldsRelated, int? RequestTypeId, string? RequestBy, string? RequestThrough);
public sealed record DecidePlatformEventRequest(string? Reason, bool EmergencyOverride, string RowVersion);
public sealed record ExecutePlatformEventRequest(string RowVersion);

public sealed record SelectionBasketResponse(Guid Id, string TargetType, string TargetId, DateTimeOffset CreatedAt, DateTimeOffset ExpiresAt);
public sealed record AddSelectionBasketRequest(string TargetType, string TargetId, int ExpiresInMinutes = 120);

public sealed record ImportDefinitionResponse(int Id, string Name, string Code, string FileMask, int? ImportSourceId, string? FilePath, string TargetHeaderTable, string? TargetLineTable, bool IsActive, int SortIndex, string RowVersion);
public sealed record SaveImportDefinitionRequest(string Name, string Code, string FileMask, int? ImportSourceId, string? FilePath, string TargetHeaderTable, string? TargetLineTable, bool IsActive, int SortIndex, string? RowVersion);
public sealed record ImportMappingGroupResponse(int Id, int ImportDefinitionId, string Name, string TargetTable, string SourcePath, bool IsRepeating);
public sealed record SaveImportMappingGroupRequest(string Name, string TargetTable, string SourcePath, bool IsRepeating);
public sealed record ImportMappingResponse(int Id, int ImportMappingGroupId, string TargetField, string SourcePath, string? SourceNode, int? MappingTypeId, int? DataTypeId, string? DefaultValue, bool IsRequired, bool IsKey, bool IsLookup, string? LookupTable, string? LookupField, string? LookupValueField, string? Format, int SortIndex, string? Description);
public sealed record SaveImportMappingRequest(string TargetField, string SourcePath, string? SourceNode, int? MappingTypeId, int? DataTypeId, string? DefaultValue, bool IsRequired, bool IsKey, bool IsLookup, string? LookupTable, string? LookupField, string? LookupValueField, string? Format, int SortIndex, string? Description);

public sealed record LanguageResponse(string Code, string? Name, bool IsActive, bool IsDefault, int? SortIndex);
public sealed record SaveLanguageRequest(string? Name, bool IsActive, bool IsDefault, int? SortIndex);
public sealed record TranslationResponse(long Id, string LanguageCode, string ResourceKey, int? ResourceId, int? CompanyId, string Value);
public sealed record SaveTranslationRequest(string LanguageCode, string ResourceKey, int? ResourceId, string Value);
public sealed record ShortListResponse(int Id, string TableName, string Caption, string? ShortName, string? Description, int IndexValue, int IndexSort, string? IndexKey);
public sealed record SaveShortListRequest(string TableName, string Caption, string? ShortName, string? Description, int IndexValue, int IndexSort, string? IndexKey);
