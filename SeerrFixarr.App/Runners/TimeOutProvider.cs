namespace SeerrFixarr.App.Runners;

public interface ITimeOutProvider
{
  Task AwaitDownloadQueueUpdatedAsync();
  Task AwaitFileDeletionAsync();
}

internal class TimeOutProvider : ITimeOutProvider
{
    public Task AwaitDownloadQueueUpdatedAsync() => Task.Delay(TimeSpan.FromSeconds(20));

    public Task AwaitFileDeletionAsync() => Task.Delay(TimeSpan.FromSeconds(10));
}