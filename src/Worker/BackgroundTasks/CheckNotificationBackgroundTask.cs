using Worker.Services;

namespace Worker.BackgroundTasks;
public class CheckNotificationBackgroundTask : IBackgroundTask
{
    private Task? _task;
    private readonly PeriodicTimer _timer = new(TimeSpan.FromMinutes(1));
    private readonly CancellationTokenSource _cts = new();
    private readonly CheckNotificationsService _checkNotificationsService;
    private ILogger<CheckNotificationBackgroundTask> _logger;

    public CheckNotificationBackgroundTask(
        CheckNotificationsService checkNotificationsService,
        ILogger<CheckNotificationBackgroundTask> logger)
    {
        _checkNotificationsService = checkNotificationsService;
        _logger = logger;
    }

    public void Start()
    {
        _task = CheckNoticiations();
    }

    private async Task CheckNoticiations()
    {
        try
        {
            while (await _timer.WaitForNextTickAsync())
            {
                var startTime = DateTime.Now;
                _logger.LogInformation("Starting CheckNotifications at {DateTime}", startTime.ToString("O"));

                var notificationsSatisfied = await _checkNotificationsService.CheckAndSendNotifications();
                _logger.LogInformation("{Satisfied} notifications satisfied", notificationsSatisfied);

                var timeElapsed = (DateTime.Now - startTime).TotalMilliseconds;
                _logger.LogInformation("Finished CheckNotifications, time elapsed : {Elapsed} ms", timeElapsed);
            }
        }
        catch (OperationCanceledException)
        {

        }
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
}
