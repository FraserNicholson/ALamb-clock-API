﻿using Shared.Clients;
using Shared.Contracts;
using Shared.Messaging;
using Shared.Models;
using Shared.Models.Database;

namespace Worker.Services;

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

        var (satisfiedNotifications, notificationIdsToDelete) = (await GetSatisfiedAndExpiredNotifications(activeNotifications));

        if (satisfiedNotifications.Any())
        {
            await _notificationProducer.SendNotifications(satisfiedNotifications);

            notificationIdsToDelete = notificationIdsToDelete.Concat(satisfiedNotifications.Select(n => n.Id)).ToList();
        }
        
        if (notificationIdsToDelete.Any())
        {
            await _databaseClient.DeleteNotifications(notificationIdsToDelete);
        }

        return satisfiedNotifications.Count;
    }

    private async Task<(ICollection<NotificationDbModel>, ICollection<string>)> GetSatisfiedAndExpiredNotifications(
        IEnumerable<NotificationDbModel> activeNotifications)
    {
        var satisfiedNotifications = new List<NotificationDbModel>();
        var expiredNotificationIds = new List<string>();

        var requiredMatchIds = activeNotifications.Select(n => n.MatchId);

        var currentMatches = await GetAllCurrentMatchesForActiveNotifications(requiredMatchIds);

        if (currentMatches == null)
        {
            return (satisfiedNotifications, expiredNotificationIds);
        }

        foreach (var notification in activeNotifications)
        {
            var notificationSatisfied = notification.NotificationType switch
            {
                NotificationType.InningsStarted
                    => GetNotificationSatisfiedForChangeOfInnings(notification, currentMatches),
                NotificationType.WicketCount
                    => GetNotificationSatisfiedForWicketCount(notification, currentMatches),
                _ => false
            };

            if (notificationSatisfied)
            {
                satisfiedNotifications.Add(notification);
                continue;
            }

            var notificationExpired = GetHasNotificationExpired(notification, currentMatches);
            if (notificationExpired)
            {
                expiredNotificationIds.Add(notification.Id);
            }
        }

        return (satisfiedNotifications, expiredNotificationIds);
    }

    private async Task<List<CricketDataCurrentMatch>> GetAllCurrentMatchesForActiveNotifications(IEnumerable<string> requiredMatchIds)
    {
        var matches = new List<CricketDataCurrentMatch>();

        var response = await _cricketDataApiClient.GetCurrentMatches();

        matches = matches.Concat(response.Data).ToList();

        var responseMatchIds = matches.Select(m => m.Id);

        var requiredMatchesNotInResponse = requiredMatchIds
            .Any(id => !responseMatchIds.Contains(id));

        if (requiredMatchesNotInResponse)
        {
            var matchesWithOffset = await _cricketDataApiClient.GetCurrentMatches(offset: 25);
            matches = matches.Concat(matchesWithOffset.Data).ToList();
        }

        return matches;
    }

    private static bool GetHasNotificationExpired(
        NotificationDbModel notification,
        IEnumerable<CricketDataCurrentMatch> currentMatches)
    {
        var match = currentMatches.SingleOrDefault(m => m.Id == notification.MatchId);

        if (match == null)
        {
            return false;
        }

        return match.MatchEnded;
    }

    private static bool GetNotificationSatisfiedForChangeOfInnings(
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

    private static bool GetNotificationSatisfiedForWicketCount(
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

    private static Score? GetCurrentInnings(CricketDataCurrentMatch? match)
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