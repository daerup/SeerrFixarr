using SeerrFixarr.Api.Radarr;
using SeerrFixarr.Api.Sonarr;
using UnitsNet;

namespace SeerrFixarr.App.Runners;

public static class FileSizeExtensionsFormatter
{
    public static string GetReadableFileSize(this MovieDownload file) => GetFileSize(file.Size);
    public static string GetReadableFileSize(this MovieFile file) => GetFileSize(file.Size);
    public static string GetReadableFileSize(this EpisodeDownload file) => GetFileSize(file.Size);
    public static string GetReadableFileSize(this EpisodeFile file) => GetFileSize(file.Size);
    
    private static string GetFileSize(long size)
    {
        var fileSize = Information.FromBytes(size);
        return $"{fileSize.Gigabytes:F2} GB";
    }
}