using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Shared.Clients;
using WebJob.Services;

namespace WebJob
{
    public class Functions
    {
        private readonly ICricketDataApiClient _cricketDataApi;
        private readonly IDbClient _dbClient;
        private readonly ICheckNotificationsService _checkNotificationsService;
        public Functions(
            ICricketDataApiClient cricketDataApi, 
            IDbClient dbClient,
            ICheckNotificationsService checkNotificationsService)
        {
            _cricketDataApi = cricketDataApi;
            _dbClient = dbClient;
            _checkNotificationsService = checkNotificationsService;
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

        [FunctionName(nameof(CheckNotifications))]
        public async Task CheckNotifications([TimerTrigger("0 * * * * *", RunOnStartup = false)] TimerInfo timerInfo, ILogger logger)
        {
            logger.LogInformation("Starting {FunctionName}", nameof(UpdateMatchesList));
            await _checkNotificationsService.CheckAndSendNotifications();
        }
    }
}