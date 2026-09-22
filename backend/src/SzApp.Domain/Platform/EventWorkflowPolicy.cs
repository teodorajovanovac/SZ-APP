namespace SzApp.Domain.Platform;

public enum PlatformEventStatus
{
    Requested = 1,
    Approved = 2,
    Rejected = 3,
    Executed = 4
}

public static class EventWorkflowPolicy
{
    public static bool CanSubmit(string role) => role is "Root" or "Upravnik" or "Moderator";

    public static void EnsureCanApprove(
        PlatformEventStatus current,
        int requestedByStaffId,
        int actorStaffId,
        string role,
        bool emergencyOverride,
        string? reason)
    {
        if (current != PlatformEventStatus.Requested) throw new InvalidOperationException("Samo zatražena promena može biti odobrena.");
        if (role is not ("Root" or "Upravnik")) throw new UnauthorizedAccessException("Uloga ne može da odobri zahtev.");
        if (requestedByStaffId == actorStaffId && !(role == "Root" && emergencyOverride && !string.IsNullOrWhiteSpace(reason)))
            throw new InvalidOperationException("Podnosilac ne može odobriti sopstveni zahtev.");
    }

    public static void EnsureCanReject(PlatformEventStatus current, string role)
    {
        if (current != PlatformEventStatus.Requested) throw new InvalidOperationException("Samo zatražena promena može biti odbijena.");
        if (role is not ("Root" or "Upravnik")) throw new UnauthorizedAccessException("Uloga ne može da odbije zahtev.");
    }

    public static void EnsureCanExecute(PlatformEventStatus current, string role)
    {
        if (current != PlatformEventStatus.Approved) throw new InvalidOperationException("Samo odobrena promena može biti izvršena.");
        if (role is not ("Root" or "Upravnik" or "Moderator")) throw new UnauthorizedAccessException("Uloga ne može da izvrši zahtev.");
    }
}

