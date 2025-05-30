using System.Runtime.Serialization;

namespace SeerrFixarr.Api.Radarr;

public enum DownloadProtocol
{
    [EnumMember(Value = "usenet")]
    Usenet,
    [EnumMember(Value = "torrent")]
    Torrent,
}