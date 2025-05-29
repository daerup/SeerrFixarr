namespace SeerrFixarr.Shared.Settings;

public record ApiSettings
{
    public string ApiKey { get; init; } = null!;
    public string ApiUrl { get; init; } = null!;

    public void Deconstruct(out string apiurl, out string apikey)
    {
        apiurl = ApiUrl;
        apikey = ApiKey;
    }
}