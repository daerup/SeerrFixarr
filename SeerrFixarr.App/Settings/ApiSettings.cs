namespace SeerrFixarr.App.Settings;

internal record ApiSettings
{
    public required string ApiKey { get; init; }
    public required string ApiUrl { get; init; }

    public void Deconstruct(out string apiurl, out string apikey)
    {
        apiurl = ApiUrl;
        apikey = ApiKey;
    }
}