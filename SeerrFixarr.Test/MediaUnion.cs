using Dunet;
using SeerrFixarr.Api.Radarr;
using SeerrFixarr.Api.Sonarr;

namespace SeerrFixarr.Test;

[Union]
internal partial record MediaUnion
{
    partial record MovieIssue(Movie Movie);

    partial record EpisodeIssue(Episode Episode);
}