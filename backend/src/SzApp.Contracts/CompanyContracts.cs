namespace SzApp.Contracts;

public sealed record CompanySummaryResponse(int Id, string Name, string? RegistrationNumber);

public sealed record CompanyContextResponse(int Id, string ShortName, string Role);
