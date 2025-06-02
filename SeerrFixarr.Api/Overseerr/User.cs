using System;

namespace SeerrFixarr.Api.Overseerr;

public record User
{
    public int Id { get; init; }
    public string Email { get; init; } = null!;
    public string PlexUsername { get; init; } = null!;
    public string Username { get; init; } = null!;
    public int PlexId { get; init; }
    public DateTime CreatedAt { get; init; }
    public int RequestCount { get; init; }
    public string DisplayName { get; init; } = null!;
}