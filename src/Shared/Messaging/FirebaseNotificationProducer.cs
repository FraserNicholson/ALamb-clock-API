using FirebaseAdmin.Messaging;
using Shared.Models;
using Shared.Models.Database;

namespace Shared.Messaging;

public interface INotificationProducer
{
    Task SendNotifications(IEnumerable<NotificationDbModel> notifications);
}
public class FirebaseNotificationProducer : INotificationProducer
{
    public async Task SendNotifications(IEnumerable<NotificationDbModel> notifications)
    {
        foreach (var notification in notifications)
        {
            var message = new MulticastMessage
            {
                Tokens = notification.RegistrationTokens,
                Notification = new Notification
                {
                    Title = GetNotificationTitle(notification),
                    Body = GetNotificationBody(notification)
                }
            };

            await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);
        }
    }

    private static string GetNotificationTitle(NotificationDbModel notification)
        => $"{notification.Team1} vs {notification.Team2}";

    private static string GetNotificationBody(NotificationDbModel notification)
    {
        return notification.NotificationType switch
        {
            NotificationType.InningsStarted => $"{notification.TeamInQuestion} have started batting",
            NotificationType.WicketCount => $"{notification.TeamInQuestion} are {notification.NumberOfWickets} down",
            _ => string.Empty
        };
    }
}