using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared.Contracts;
using Shared.Options;

namespace Shared.Clients
{
    public interface ICricketDataApiClient
    {
        Task<CricketDataMatch[]> GetMatches();
        Task<CricketDataCurrentMatch[]> GetCurrentMatches();
    }
    
    public class CricketDataApiClient : ICricketDataApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly CricketDataApiOptions _apiOptions;

        public CricketDataApiClient(HttpClient httpClient, IOptions<CricketDataApiOptions> apiOptions)
        {
            _httpClient = httpClient;
            _apiOptions = apiOptions.Value;
        }
        
        public async Task<CricketDataMatch[]> GetMatches()
        {
            var requestUri = ConstructRequestUri("cricScore");

            var response = (await Get<CricketDataMatchesResponse>(requestUri))!;
            return response.Data;
        }

        public async Task<CricketDataCurrentMatch[]> GetCurrentMatches()
        {
            var requestUri = ConstructRequestUri("currentMatches");

            var response = (await Get<CricketDataCurrentMatchesResponse>(requestUri))!;
            return response.Data;
        }

        private async Task<T?> Get<T>(string requestUri)
        {
            var response = await _httpClient.GetAsync(requestUri);
            var x = JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
            return x;
        }

        private string ConstructRequestUri(string endpointPath) => $"{endpointPath}?apikey={_apiOptions.ApiKey}";
    }
}