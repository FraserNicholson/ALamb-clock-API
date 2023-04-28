using Newtonsoft.Json;

namespace Shared.Contracts
{
    public class CricketDataCurrentMatch
    {
        public string Id { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// Start dateTime of the match
        /// </summary>
        public DateTime DateTimeGMT { get; set; }
        public string MatchType { get; set; }
        public string Status { get; set; }
        public string[] Teams { get; set; }
        public Score[] Score { get; set; }
        public string Venue { get; set; }
        [JsonProperty("Series_id")]
        public string SeriesId { get; set; }
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
        public string Inning { get; set; }
    }
}