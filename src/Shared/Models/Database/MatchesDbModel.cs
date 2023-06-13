using MongoDB.Bson.Serialization.Attributes;

namespace Shared.Models.Database;

public class MatchesDbModel
{
    [BsonId]
    public string Id { get; set; } = default!;
    public string DateStored { get; init; } = default!;
    public MatchDbModel[] Matches { get; set; } = default!;
}

public class MatchDbModel
{
    [BsonId]
    public string Id { get; set; } = default!;
    public DateTime DateTimeGmt { get; set; } = default!;
    public string MatchType { get; set; } = default!;
    public string Status { get; set; } = default!;
    /// <summary>
    /// Describes the state the match is currently in, can be result/live/fixture
    /// </summary>
    public MatchStatus MatchStatus { get; set; }
    public string Team1 { get; set; } = default!;
    public string Team2 { get; set; } = default!;
}
    
public enum MatchStatus
{
    Result,
    Live,
    Fixture
}