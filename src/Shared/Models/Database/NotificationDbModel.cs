using MongoDB.Bson.Serialization.Attributes;

namespace Shared.Models.Database;

public class NotificationDbModel
{
    [BsonId]
    public string Id { get; set; } = default!;
    public string MatchId { get; set; } = default!;
    public string Team1 { get; set; } = default!;
    public string Team2 { get; set; } = default!;
    public DateTime DateTimeGmt { get; set; }
    public string TeamInQuestion { get; set; } = default!;
    public NotificationType NotificationType { get; set; }
    public int? NumberOfWickets { get; set; }
    public List<string> RegistrationTokens { get; set; } = default!;
}