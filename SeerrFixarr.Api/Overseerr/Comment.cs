namespace SeerrFixarr.Api.Overseerr;

public record Comment
{
    public int Id { get; init; }
    public required string Message { get; init; }
    public DateTime CreatedAt { get; init; }
    public User User { get; init; } = null!;
    
    public static implicit operator Comment(string message) => new() { Message = message };
    
}