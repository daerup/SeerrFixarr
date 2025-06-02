using System.Runtime.Serialization;

namespace SeerrFixarr.Api.Shared;

public enum DownloadProtocol
{
    [EnumMember(Value = "usenet")]
    Usenet,
    [EnumMember(Value = "torrent")]
    Torrent,
}