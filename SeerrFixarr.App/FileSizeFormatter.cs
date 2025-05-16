using UnitsNet;

namespace SeerrFixarr.App;

public class FileSizeFormatter
{
    public string GetFileSize(long size)
    {
        var fileSize = Information.FromBytes(size);
        return $"{fileSize.Gigabytes:F2} GB";
    }
}