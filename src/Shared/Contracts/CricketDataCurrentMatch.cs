using Newtonsoft.Json;

namespace Shared.Contracts
{
    public class CricketDataCurrentMatch
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        /// <summary>
        /// Start dateTime of the match
        /// </summary>
        public DateTime DateTimeGmt { get; set; } = default!;
        public string MatchType { get; set; } = default!;
        public string Status { get; set; } = default!;
        public string[] Teams { get; set; } = default!;
        public Score[] Score { get; set; } = default!;
        public string Venue { get; set; } = default!;
        [JsonProperty("Series_id")]
        public string SeriesId { get; set; } = default!;
        public bool MatchStarted { get; set; }
        public bool MatchEnded { get; set; }
    }

    public class Score
    {
        [JsonProperty("R")]
        public int Runs { get; set; }
        [JsonProperty("W")]
        public int Wickets { get; set; }
        [JsonProperty("O")]
        public double Overs { get; set; }
        public string Inning { get; set; } = default!;
    }
}