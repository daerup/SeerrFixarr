namespace SeerrFixarr.Shared.Settings;

public record SeerrFixarrSettings
{
    public ApiSettings Overseerr { get; init; } = null!;
    public ApiSettings Radarr { get; init; } = null!;
    public ApiSettings Sonarr { get; init; } = null!;
}