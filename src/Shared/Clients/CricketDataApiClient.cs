﻿using Microsoft.Extensions.Options;
using Shared.Contracts;
using Shared.Options;

namespace Shared.Clients;

public interface ICricketDataApiClient
{
    Task<CricketDataMatchesResponse> GetMatches();
    Task<CricketDataCurrentMatchesResponse> GetCurrentMatches(int offset = 0);
}
    
public class CricketDataApiClient : BaseHttpClient, ICricketDataApiClient
{
    private readonly CricketDataApiOptions _apiOptions;

    public CricketDataApiClient(HttpClient httpClient, IOptions<CricketDataApiOptions> apiOptions) : base(httpClient)
    {
        _apiOptions = apiOptions.Value;
    }
        
    public async Task<CricketDataMatchesResponse> GetMatches()
    {
        var requestUri = ConstructRequestUri("cricScore");

        var response = (await Get<CricketDataMatchesResponse>(requestUri))!;
        return response;
    }

    public async Task<CricketDataCurrentMatchesResponse> GetCurrentMatches(int offset = 0)
    {
        var requestUri = ConstructRequestUri("currentMatches");

        requestUri += $"&offset={offset}";


        var response = (await Get<CricketDataCurrentMatchesResponse>(requestUri))!;
        return response;
    }

    private string ConstructRequestUri(string endpointPath) => $"{endpointPath}?apikey={_apiOptions.ApiKey}";
}