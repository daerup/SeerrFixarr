namespace SeerrFixarr.Api.Sonarr;

public record SearchEpisodeRequest
{
    public string Name => "EpisodeSearch";
    public int[] EpisodeIds { get; init; } = [];
    public static implicit operator SearchEpisodeRequest(int episodeId) => new() { EpisodeIds = [episodeId] };
}