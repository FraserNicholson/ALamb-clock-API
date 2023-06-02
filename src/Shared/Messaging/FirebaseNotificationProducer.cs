using FirebaseAdmin.Messaging;
using Shared.Models.Database;

namespace Shared.Messaging
{
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
                var message = new MulticastMessage()
                {
                    Tokens = notification.RegistrationTokens,
                    Data = new Dictionary<string, string>()
                    {
                        { "NotificationId", notification.Id },
                    }
                };

                await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);
            }
        }
    }
}
