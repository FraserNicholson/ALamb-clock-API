using MongoDB.Bson.Serialization.Attributes;
using Shared.Contracts;

namespace Shared.Models.Database
{
    public class MatchesDbModel : CricketDataMatchesResponse
    {
        [BsonId]
        public string Id { get; set; } = default!;
    }
}