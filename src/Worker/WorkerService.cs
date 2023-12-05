using Worker.Services;
using Shared.Clients;
using Worker.BackgroundTasks;

namespace Worker;

public class WorkerService : BackgroundService
{
    private readonly ILogger<WorkerService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public WorkerService(
        ILogger<WorkerService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Worker");
        _logger.LogInformation("Setting backgroung jobs");

        using var scope = _serviceProvider.CreateScope();

        var checkNotificationsTask = new CheckNotificationBackgroundTask(
            scope.ServiceProvider.GetRequiredService<CheckNotificationsService>(),
            scope.ServiceProvider.GetRequiredService<ILogger<CheckNotificationBackgroundTask>>());

        checkNotificationsTask.Start();

        var updateMatchesTask = new UpdateMatchesBackgroundTask(
            scope.ServiceProvider.GetRequiredService<ICricketDataApiClient>(),
            scope.ServiceProvider.GetRequiredService<IDbClient>(),
            scope.ServiceProvider.GetRequiredService<ILogger<UpdateMatchesBackgroundTask>>());

        updateMatchesTask.Start();

        while (!cancellationToken.IsCancellationRequested)
        {
        }

        await checkNotificationsTask.StopAsync();
        await updateMatchesTask.StopAsync();
    }
}
