using Shared.Models;

namespace Shared.Contracts;

public class NotificationResponse
{
    public string Id { get; set; } = default!;
    public string MatchId { get; set; } = default!;
    public string Team1 { get; set; } = default!;
    public string Team2 { get; set; } = default!;
    public DateTimeOffset MatchStartsAt { get; set; }
    public string TeamInQuestion { get; set; } = default!;
    public NotificationType NotificationType { get; set; }
    public int? NumberOfWickets { get; set; }
}