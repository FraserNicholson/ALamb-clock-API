using MongoDB.Bson.Serialization.Attributes;

namespace Shared.Models.Database
{
    public class NotificationDbModel
    {
        [BsonId]
        public string Id { get; set; } = default!;
        public string MatchId { get; set; } = default!;
        public DateTimeOffset MatchStartsAt { get; set; } = default!;
        public string TeamInQuestion { get; set; } = default!;
        public NotificationType NotificationType { get; set; }
        public int? NumberOfWickets { get; set; }
        public List<string> RegistrationTokens { get; set; } = default!;
    }
}
