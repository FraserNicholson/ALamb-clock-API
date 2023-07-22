namespace Shared.Models;

public class SaveNotificationDbRequest
{
    public string? NotificationId { get; set; }
    public string RegistrationToken { get; set; } = default!;
    public string MatchId { get; set; } = default!;
    public string TeamInQuestion { get; set; } = default!;
    public NotificationType NotificationType { get; set; }
    public int? NumberOfWickets { get; set; }
}