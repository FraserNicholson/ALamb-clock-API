using Shared.Models;

namespace Shared.Contracts;

public class AddNotificationRequest
{
    public string RegistrationToken { get; set; } = default!;
    public string MatchId { get; set; } = default!;
    public string Team1 { get; set; } = default!;
    public string Team2 { get; set; } = default!;
    public DateTimeOffset MatchStartsAt { get; set; } = default!;
    public string TeamInQuestion { get; set; } = default!;
    public NotificationType NotificationType { get; set; }
    public int? NumberOfWickets { get; set; }
}