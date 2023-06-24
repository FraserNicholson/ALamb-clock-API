using Shared.Clients;
using Shared.Contracts;
using Shared.Messaging;
using Shared.Models;
using Shared.Models.Database;

namespace WebJob.Services;

public interface ICheckNotificationsService
{
    Task<int> CheckAndSendNotifications();
}
public class CheckNotificationsService : ICheckNotificationsService
{
    private readonly IDbClient _databaseClient;
    private readonly ICricketDataApiClient _cricketDataApiClient;
    private readonly INotificationProducer _notificationProducer;
    public CheckNotificationsService(
        IDbClient databaseClient,
        ICricketDataApiClient cricketDataApiClient,
        INotificationProducer notificationProducer)
    {
        _databaseClient = databaseClient;
        _cricketDataApiClient = cricketDataApiClient;
        _notificationProducer = notificationProducer;
    }

    public async Task<int> CheckAndSendNotifications()
    {
        var activeNotifications = (await _databaseClient.GetActiveNotifications()).ToList();

        if (!activeNotifications.Any())
        {
            return 0;
        }

        var satisfiedNotifications = (await GetSatisfiedNotifications(activeNotifications)).ToList();

        if (!satisfiedNotifications.Any())
        {
            return 0;
        }
        
        await _notificationProducer.SendNotifications(satisfiedNotifications);

        await _databaseClient.DeleteNotifications(satisfiedNotifications.Select(n => n.Id));

        return satisfiedNotifications.Count;
    }

    private async Task<IEnumerable<NotificationDbModel>> GetSatisfiedNotifications(
        IEnumerable<NotificationDbModel> activeNotifications)
    {
        var satisfiedNotificationIds = new List<NotificationDbModel>();
        var currentMatches = await _cricketDataApiClient.GetCurrentMatches();

        foreach (var notification in activeNotifications)
        {
            var notificationSatisfied = notification.NotificationType switch
            {
                NotificationType.InningsStarted
                    => GetNotificationSatisfiedForChangeOfInnings(notification, currentMatches.Data),
                NotificationType.WicketCount
                    => GetNotificationSatisfiedForWicketCount(notification, currentMatches.Data),
                _ => false
            };

            if (notificationSatisfied)
            {
                satisfiedNotificationIds.Add(notification);
            }
        }

        return satisfiedNotificationIds;
    }

    private bool GetNotificationSatisfiedForChangeOfInnings(
        NotificationDbModel notification,
        IEnumerable<CricketDataCurrentMatch> currentMatches)
    {
        var match = currentMatches.SingleOrDefault(m => m.Id == notification.MatchId);

        var currentInnings = GetCurrentInnings(match);

        if (currentInnings == null)
        {
            return false;
        }

        if (currentInnings.Inning.Contains(notification.TeamInQuestion, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return currentInnings.Wickets == 10;
    }

    private bool GetNotificationSatisfiedForWicketCount(
        NotificationDbModel notification,
        IEnumerable<CricketDataCurrentMatch> currentMatches)
    {
        var match = currentMatches.SingleOrDefault(m => m.Id == notification.MatchId);

        var currentInnings = GetCurrentInnings(match);

        if (currentInnings == null)
        {
            return false;
        }

        if (!currentInnings.Inning.Contains(notification.TeamInQuestion, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return currentInnings.Wickets >= notification.NumberOfWickets;
    }

    private Score? GetCurrentInnings(CricketDataCurrentMatch? match)
    {
        if (match == null)
        {
            return null;
        }

        if (match.MatchEnded)
        {
            return null;
        }

        if (!match.MatchStarted)
        {
            return null;
        }

        return match.Score.LastOrDefault();
    }
}