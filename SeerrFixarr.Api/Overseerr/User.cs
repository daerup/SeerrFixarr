namespace SeerrFixarr.Api.Overseerr;

public record User
{
    public int Permissions { get; init; }
    public int Id { get; init; }
    public string Email { get; init; }
    public string PlexUsername { get; init; }
    public string Username { get; init; }
    public int PlexId { get; init; }
    public DateTime CreatedAt { get; init; }
    public int RequestCount { get; init; }
    public string DisplayName { get; init; }
}