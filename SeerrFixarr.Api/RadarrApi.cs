using GeneratedClients;
using SeerrFixarr.Api.GeneratedClients;

namespace SeerrFixarr.Api;

public class OverseerrApi(IHttpClientFactory httpClientFactory, string apiUrl, string apiKey)
{
    private readonly OverseerrRestClient _restClient = new(new GeneratedClientOptions(apiUrl, apiKey, httpClientFactory));

    public Task GetIssue()
    {
        throw new NotImplementedException();
    }
}

            
public class SonarrApi(IHttpClientFactory httpClientFactory, string apiUrl, string apiKey)
{
    private readonly SonarrRestClient _restClient = new(new GeneratedClientOptions(apiUrl, apiKey, httpClientFactory));

    public Task DeleteEpisodeFile()
    {
        throw new NotImplementedException();
    }
}

            

public class RadarrApi(IHttpClientFactory httpClientFactory, string apiUrl, string apiKey)
{
    private readonly RadarrRestClient _restClient = new(new GeneratedClientOptions(apiUrl, apiKey, httpClientFactory));

    public Task DeleteMovieFile() 
    {
        throw new NotImplementedException();
    }
}

