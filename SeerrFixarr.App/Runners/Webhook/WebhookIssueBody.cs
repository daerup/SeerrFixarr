using System.Text.Json.Serialization;

namespace SeerrFixarr.App.Runners.Webhook;

public record WebhookIssueBody
{
    [JsonPropertyName("notification_type")] public string NotificationType { get; init; } = null!;
    [JsonPropertyName("issue_description")] public string IssueDescription { get; init; } = null!;
    public WebhookIssueRoot IssueRoot { get; init; } = null!;
}