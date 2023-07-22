using NotificationTypeEnum = Shared.Models.NotificationType;

namespace Shared.Contracts;

public class SaveNotificationRequest
{
    public string? NotificationId { get; set; }
    public string MatchId { get; set; } = default!;
    public string TeamInQuestion { get; set; } = default!;
    public string NotificationType { get; set; } = default!;
    public int? NumberOfWickets { get; set; }

    public (bool, string) IsValid()
    {
        if (string.IsNullOrWhiteSpace(MatchId))
        {
            return (false, ErrorMessage(nameof(MatchId)));
        }
        
        if (string.IsNullOrWhiteSpace(TeamInQuestion))
        {
            return (false, ErrorMessage(nameof(TeamInQuestion)));
        }

        var notificationTypeIsValid =
            Enum.TryParse<NotificationTypeEnum>(NotificationType, ignoreCase: true, out var notificationTypeEnumValue);

        if (!notificationTypeIsValid)
        {
            return (false, "Notification type is invalid. Accepted values are: 'InningsStarted', 'WicketCount'");
        }

        if (notificationTypeEnumValue == NotificationTypeEnum.WicketCount && NumberOfWickets is null)
        {
            return (false,
                $"{nameof(NumberOfWickets)} must be provided when notification is of type {NotificationType}");
        }

        return (true, string.Empty);
    }

    private static string ErrorMessage(string propertyName)
        => $"Field {propertyName} must be provided";
}