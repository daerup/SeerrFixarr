using Refit;

namespace SeerrFixarr.Api.Radarr;

public interface IRadarrApi
{
    [Get("/movie/{id}")]
    Task GetMovie([AliasAs("id")] int movieId);
}