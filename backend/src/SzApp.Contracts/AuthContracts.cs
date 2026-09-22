namespace SzApp.Contracts;

public sealed record LoginRequest(string Email, string Password, bool RememberMe = false);

public sealed record CurrentUserResponse(
    int Id,
    string DisplayName,
    string Email,
    string PreferredLanguage,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<CompanySummaryResponse> Companies);

public sealed record AntiforgeryTokenResponse(string Token, string HeaderName);
