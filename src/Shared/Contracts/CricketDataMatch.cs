using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Shared.Contracts
{
    public class CricketDataMatch
    {
        public string Id { get; set; } = default!;
        public DateTime DateTimeGmt { get; set; } = default!;
        public string MatchType { get; set; } = default!;
        public string Status { get; set; } = default!;
        /// <summary>
        /// Describes the state the match is currently in, can be result/live/fixture
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("MS")]
        public MatchStatus MatchStatus { get; set; }
        [JsonProperty("T1")]
        public string Team1 { get; set; } = default!;
        [JsonProperty("T2")]
        public string Team2 { get; set; } = default!;
    }
    
    public enum MatchStatus
    {
        Result,
        Live,
        Fixture
    }
}