using Shared.Contracts;
using Shared.Models;
using Shared.Models.Database;

namespace Shared.Extensions;

public static class NotificationModelsExtensions
{
    public static NotificationResponse ToResponse(this NotificationDbModel dbModel)
    {
        return new NotificationResponse
        {
            Id = dbModel.Id,
            MatchId = dbModel.MatchId,
            Team1 = dbModel.Team1,
            Team2 = dbModel.Team2,
            MatchStartsAt = dbModel.DateTimeGmt,
            NotificationType = dbModel.NotificationType.ToFriendlyString(),
            TeamInQuestion = dbModel.TeamInQuestion,
            NumberOfWickets = dbModel.NumberOfWickets
        };
    }

    public static IEnumerable<NotificationResponse> ToResponse(this IEnumerable<NotificationDbModel> notifications)
    {
        return notifications.Select(n => n.ToResponse());
    }

    private static string ToFriendlyString(this NotificationType notificationType)
    {
        var value = notificationType switch
        {
            NotificationType.InningsStarted => "Innings Started",
            NotificationType.WicketCount => "Wicket Count",
            _ => throw new ArgumentOutOfRangeException(nameof(notificationType), notificationType, null)
        };
        
        return value;
    }
}