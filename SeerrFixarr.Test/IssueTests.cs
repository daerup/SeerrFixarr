using SeerrFixarr.Api.Overseerr;
using Shouldly;

namespace SeerrFixarr.Test;

public class IssueTests
{
    [Fact]
    public void GetIdentifier_ShouldReturnTvIdentifier_WhenMediaIsTv_WithPaddedSeasonAndEpisode()
    {
        var issue = new Issue
        {
            ProblemSeason = 2,
            ProblemEpisode = 5,
            Media = new Media
            {
                MediaType = MediaType.Tv
            }
        };

        var result = issue.GetIdentifier();

        result.ShouldBe("S02E05");
    }

    [Fact]
    public void GetIdentifier_ShouldReturnTvIdentifier_WhenMediaIsTv()
    {
        var issue = new Issue
        {
            ProblemSeason = 12,
            ProblemEpisode = 13,
            Media = new Media
            {
                MediaType = MediaType.Tv
            }
        };
        
        var result = issue.GetIdentifier();
        
        result.ShouldBe("S12E13");
    }

    [Fact]
    public void GetIdentifier_ShouldReturnMovieIdentifier_WhenMediaIsMovie()
    {
        var issue = new Issue
        {
            Media = new Media
            {
                MediaType = MediaType.Movie,
                TmdbId = 12345
            }
        };

        var result = issue.GetIdentifier();

        result.ShouldBe("12345");
    }

    [Fact]
    public void GetIdentifier_ShouldThrow_WhenMediaTypeIsTvButSeasonOrEpisodeIsNull()
    {
        var issue = new Issue
        {
            ProblemSeason = null,
            ProblemEpisode = 3,
            Media = new Media
            {
                MediaType = MediaType.Tv
            }
        };

        Should.Throw<InvalidOperationException>(() => issue.GetIdentifier());
    }

    [Fact]
    public void GetIdentifier_ShouldThrow_WhenMediaIsNull()
    {
        var issue = new Issue
        {
            Media = null!
        };

        Should.Throw<NullReferenceException>(() => issue.GetIdentifier());
    }
}