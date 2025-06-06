namespace SeerrFixarr.Shared.Settings;

public record SeerrFixarrSettings
{
    public ApiSettings Overseerr { get; init; } = null!;
    public ApiSettings Radarr { get; init; } = null!;
    public ApiSettings Sonarr { get; init; } = null!;
    public string JwtSigningKey { get; init; } = null!;
    public string ExternalHost { get; init; } = null!;
    public Dictionary<string, List<string>> UserRedirectKeyPool
    {
        get;
        init => field = new Dictionary<string, List<string>>(value, StringComparer.OrdinalIgnoreCase);
    } = null!;
}