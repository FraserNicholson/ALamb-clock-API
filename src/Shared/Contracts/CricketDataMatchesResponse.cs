using Newtonsoft.Json;

namespace Shared.Contracts;

public class CricketDataMatchesResponse
{
    [JsonIgnore]
    public string DateStored { get; set; } = default!;
    public CricketDataMatch[] Data { get; set; } = default!;
}