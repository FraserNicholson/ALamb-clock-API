using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Shared.Clients;

namespace WebJob
{
    public class Functions
    {
        private readonly ICricketDataApiClient _cricketDataApi;
        private readonly IDbClient _dbClient;
        public Functions(ICricketDataApiClient cricketDataApi, IDbClient dbClient)
        {
            _cricketDataApi = cricketDataApi;
            _dbClient = dbClient;
        }
        
        [FunctionName(nameof(UpdateMatchesList))]
        public async Task UpdateMatchesList([TimerTrigger("0 0 2 * * *", RunOnStartup = false)] TimerInfo timerInfo, ILogger logger)
        {
            logger.LogInformation("Starting {FunctionName}", nameof(UpdateMatchesList));
            logger.LogInformation("Fetching latest cricket matches list");
            var cricketMatches = await _cricketDataApi.GetMatches();
            
            logger.LogInformation("Storing cricket matches list");
            await _dbClient.SaveCricketMatches(cricketMatches);
            
            logger.LogInformation("Removing expired matches");
            await _dbClient.DeleteExpiredCricketMatches();
        }
    }
}