using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Shared.Contracts
{
    public class CricketDataMatch
    {
        public string Id { get; set; }
        public DateTime DateTimeGMT { get; set; }
        public string MatchType { get; set; }
        public string Status { get; set; }
        /// <summary>
        /// Describes the state the match is currently in, can be result/live/fixture
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public MatchStatus MatchStatus { get; set; }
        [JsonProperty("T1")]
        public string Team1 { get; set; }
        [JsonProperty("T2")]
        public string Team2 { get; set; }
    }
    
    public enum MatchStatus
    {
        Result,
        Live,
        Fixture
    }
}