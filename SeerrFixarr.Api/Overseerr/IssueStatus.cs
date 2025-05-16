using System.Runtime.Serialization;

namespace SeerrFixarr.Api.Overseerr;

public enum IssueStatus
{
    [EnumMember(Value = "open")]
    Open,
    [EnumMember(Value = "resolved")]
    Resolved,
}