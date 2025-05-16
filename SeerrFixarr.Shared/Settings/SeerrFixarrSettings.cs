namespace SeerrFixarr.Shared.Settings;

public record SeerrFixarrSettings
{
    public required ApiSettings Overseerr { get; init; }
    public required ApiSettings Radarr { get; init; }
    public required ApiSettings Sonarr { get; init; }
}