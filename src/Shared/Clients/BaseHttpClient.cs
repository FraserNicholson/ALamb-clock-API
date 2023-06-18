using Newtonsoft.Json;

namespace Shared.Clients;

public class BaseHttpClient
{
    private readonly HttpClient _httpClient;

    protected BaseHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    protected async Task<T?> Get<T>(string requestUri)
    {
        var response = await _httpClient.GetAsync(requestUri);
        return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
    }
}