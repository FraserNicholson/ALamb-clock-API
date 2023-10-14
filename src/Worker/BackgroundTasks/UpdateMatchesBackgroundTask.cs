using Shared.Clients;

namespace Worker.BackgroundTasks;
public class UpdateMatchesBackgroundTask : BackgroundTask
{
    private Task? _task;
    private readonly PeriodicTimer _timer = new(TimeSpan.FromDays(1));
    private readonly CancellationTokenSource _cts = new();
    private readonly ICricketDataApiClient _cricketDataApi;
    private readonly IDbClient _dbClient;
    private readonly ILogger<UpdateMatchesBackgroundTask> _logger;

    public UpdateMatchesBackgroundTask(
        ICricketDataApiClient cricketDataApi,
        IDbClient dbClient,
        ILogger<UpdateMatchesBackgroundTask> logger)
    {
        _cricketDataApi = cricketDataApi;
        _dbClient = dbClient;
        _logger = logger;
    }

    public async void Start()
    {
        await Task.Delay(180 * 1000);

        _task = DoWork();
    }

    private async Task DoWork()
    {
        // Call once to ensure we don't miss an invocation because of how timers work
        await UpdateMatches();

        try
        {
            while (await _timer.WaitForNextTickAsync())
            {
                await UpdateMatches();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task UpdateMatches()
    {
        var startTime = DateTime.Now;
        _logger.LogInformation("Starting UpdateMatches at {DateTime}", startTime.ToString("O"));

        _logger.LogInformation("Fetching latest cricket matches list");
        var cricketMatches = await _cricketDataApi.GetMatches();

        _logger.LogInformation("Storing cricket matches list");
        await _dbClient.SaveCricketMatches(cricketMatches);

        _logger.LogInformation("Removing expired matches");
        await _dbClient.DeleteExpiredCricketMatches();

        var timeElapsed = (DateTime.Now - startTime).TotalMilliseconds;
        _logger.LogInformation("Finished UpdateMatches, time elapsed : {Elapsed} ms", timeElapsed);
    }

    public async Task StopAsync()
    {
        if (_task is null)
        {
            return;
        }

        _cts.Cancel();
        await _task;
        _cts.Dispose();
    }

    public static int CalculateMillisecondsUntilNextOcurrenceOfHour(int hour)
    {
        var today = DateTime.Now;

        var tomorrow = today.Add(new TimeSpan(1, 0, 0, 0));

        var tomorrowAtSix = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, hour, 0, 0);

        var diff = tomorrowAtSix.Subtract(DateTime.Now);

        if (diff.TotalHours > 24d) // next ocurrence is tomorrow
        {
            return (int)(diff.TotalMilliseconds - (24 * 60 * 60 * 1000));
        }
        else  // next ocurrence is today
        {
            return (int)diff.TotalMilliseconds;
        }
    }
}
