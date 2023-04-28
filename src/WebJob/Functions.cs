using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Shared.Clients;

namespace WebJob
{
    public class Functions
    {
        private readonly ICricketDataApiClient _cricketDataApi;
        public Functions(ICricketDataApiClient cricketDataApi)
        {
            _cricketDataApi = cricketDataApi;
        }
        
        [FunctionName(nameof(UpdateMatchesList))]
        public async Task UpdateMatchesList([TimerTrigger("0 0 * * * *", RunOnStartup = true)] TimerInfo timerInfo, ILogger logger)
        {
            logger.LogInformation("Starting {FunctionName}", nameof(UpdateMatchesList));
            logger.LogInformation("Fetching latest cricket matches list");
            var matches = await _cricketDataApi.GetMatches();
        }
    }
}