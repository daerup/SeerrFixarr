using System.Text.Json.Serialization;

namespace SeerrFixarr.App.Runners.Webhook;

public record WebhookIssueRoot
{
    [JsonPropertyName("issue_id")] 
    public int IssueId { get; init; }
    
    [JsonPropertyName("reportedBy_username")] 
    public string ReportedByUsername { get; init; } = null!;
    
    [JsonPropertyName("notification_type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public NotificationType NotificationType { get; init; }
}