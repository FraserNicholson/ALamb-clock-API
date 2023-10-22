using Shared.Models;

namespace Shared.Contracts;

public class NotificationResponse
{
    public string Id { get; set; } = default!;
    public string MatchId { get; set; } = default!;
    public string Team1 { get; set; } = default!;
    public string Team2 { get; set; } = default!;
    public string MatchStartsAt { get; set; }
    public string TeamInQuestion { get; set; } = default!;
    public string NotificationType { get; set; } = default!;
    public int? NumberOfWickets { get; set; }
}