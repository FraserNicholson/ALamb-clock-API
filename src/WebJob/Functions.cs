using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace WebJob
{
    public static class Functions
    {
        [FunctionName(nameof(UpdateMatchesList))]
        public static void UpdateMatchesList([TimerTrigger("0 * * * * *", RunOnStartup = true)] TimerInfo timerInfo, ILogger logger)
        {
            logger.LogInformation("Running now!");
        }
    }
}