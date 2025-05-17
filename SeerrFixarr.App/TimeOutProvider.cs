namespace SeerrFixarr.App;

internal class TimeOutProvider : ITimeOutProvider
{
    public Task AwaitDownloadQueueUpdated()
    {
        return Task.Delay(TimeSpan.FromSeconds(10));
    }
    
    public Task AwaitFileDeletion()
    {
        return Task.Delay(TimeSpan.FromSeconds(5));
    }
}