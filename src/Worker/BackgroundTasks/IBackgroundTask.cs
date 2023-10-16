namespace Worker.BackgroundTasks;
public interface IBackgroundTask
{
    public void Start();
    public Task StopAsync();
}
