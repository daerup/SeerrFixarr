namespace SeerrFixarr.App.Settings;

internal record SeerrFixarrSettings
{
    public required ApiSettings Overseerr { get; init; }
    public required ApiSettings Radarr { get; init; }
    public required ApiSettings Sonarr { get; init; }
}