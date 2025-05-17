namespace SeerrFixarr.App;

public interface ITimeOutProvider
{
    Task AwaitDownloadQueueUpdated();
    Task AwaitFileDeletion();
}