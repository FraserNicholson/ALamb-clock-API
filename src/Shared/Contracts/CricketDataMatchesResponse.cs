using Newtonsoft.Json;

namespace Shared.Contracts
{
    public class CricketDataMatchesResponse
    {
        public CricketDataMatch[] Data { get; set; }
        [JsonIgnore]
        public string DateStored { get; set; }
    }
}