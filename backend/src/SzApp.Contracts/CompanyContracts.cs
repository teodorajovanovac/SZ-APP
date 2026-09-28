namespace SzApp.Contracts;

public sealed record CompanySummaryResponse(int Id, string Name, string? RegistrationNumber, string? LocationName = null);

public sealed record CompanyContextResponse(int Id, string ShortName, string Role);
