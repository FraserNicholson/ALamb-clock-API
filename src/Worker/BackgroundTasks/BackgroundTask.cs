namespace Worker.BackgroundTasks;
public interface BackgroundTask
{
    public void Start();
    public Task StopAsync();
}
