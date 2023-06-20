using Shared.Models;

namespace Shared.Contracts;

public class AddNotificationRequest
{
    public string RegistrationToken { get; set; } = default!;
    public string MatchId { get; set; } = default!;
    public string Team1 { get; set; } = default!;
    public string Team2 { get; set; } = default!;
    public DateTime DateTimeGmt { get; set; } = default!;
    public string TeamInQuestion { get; set; } = default!;
    public NotificationType NotificationType { get; set; }
    public int? NumberOfWickets { get; set; }

    public (bool, string) IsValid()
    {
        if (string.IsNullOrWhiteSpace(RegistrationToken))
        {
            return (false, ErrorMessage(nameof(RegistrationToken)));
        }
        
        if (string.IsNullOrWhiteSpace(MatchId))
        {
            return (false, ErrorMessage(nameof(MatchId)));
        }
        
        if (string.IsNullOrWhiteSpace(Team1))
        {
            return (false, ErrorMessage(nameof(Team1)));
        }
        
        if (string.IsNullOrWhiteSpace(Team2))
        {
            return (false, ErrorMessage(nameof(Team2)));
        }
        
        if (string.IsNullOrWhiteSpace(TeamInQuestion))
        {
            return (false, ErrorMessage(nameof(TeamInQuestion)));
        }

        if (NotificationType == NotificationType.WicketCount && NumberOfWickets is null)
        {
            return (false,
                $"{nameof(NumberOfWickets)} must be provided when notification is of type {nameof(NotificationType.WicketCount)}");
        }

        return DateTimeGmt <= DateTime.Now ? (false, $"{nameof(DateTimeGmt)} must be in the future") : (true, string.Empty);
    }

    private static string ErrorMessage(string propertyName)
        => $"Field {propertyName} must be provided";
}