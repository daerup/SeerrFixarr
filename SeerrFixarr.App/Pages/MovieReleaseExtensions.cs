using SeerrFixarr.Api.Radarr;
using Serilog;

namespace SeerrFixarr.App.Pages;

internal static class MovieReleaseExtensions
{
    public static async Task<MovieRelease[]> GetDummyData(this IRadarrApi _, int movieId)
    {
        Log.Information("Fetching");
        await Task.Delay(2000); // simulate API delay
        // await Task.Delay(800000); // simulate API delay
        Log.Information("Fetched");

        var length = new Random().Next(10, 100);
        // var length = 0;
        return Enumerable.Range(0, length) // Random number of releases between 1 and 10
            .Select(_ => MovieRelease(movieId))
            .ToArray();
    }

    private static MovieRelease MovieRelease(int movieId)
    {
        var random = new Random();
        var randomAge = random.Next(1, 30); // Random age between 1 and 30 days
        var randomSizeByte = (long)UnitsNet.Information.FromGigabytes( random.Next(1, 10)).Bytes;
        var guid = Guid.NewGuid().ToString();
        var rejections = Enumerable.Range(0, random.Next(0, 3)) // Random number of rejections between 0 and 2
            .Select(_ => $"Rejection {Guid.NewGuid().ToString()[..5]}")
            .ToList();
        return new MovieRelease
        {
            Guid = guid,
            Title = $"Example Release {guid[..5]}",
            Age = randomAge, // days
            AgeHours = randomAge * 24, // hours
            AgeMinutes = randomAge * 24 * 60, // minutes
            Size = randomSizeByte,
            CustomFormatScore = random.Next(0, 10),
            InfoUrl = $"https://example.com/release/{guid}",
            Quality = new QualityRevision
            {
                Quality = new Quality { Name = "HD", Resolution = 1080 },
                Revision = new Revision { Version = 1, Real = 1 }
            },
            Languages = GetLanguages(random),
            Indexer = "ExampleIndexer",
            IndexerId = 1,
            MappedMovieId = movieId,
            ImdbId = movieId,
            DownloadProtocol = random.Next(0, 2) == 0 ? DownloadProtocol.Torrent : DownloadProtocol.Usenet,
            Rejections = rejections
        };
    }

    private static List<Language> GetLanguages(Random random)
    {
        var english = new Language() { Name = "English"};
        var german = new Language() { Name = "German" };
        return random.Next(0, 3) switch
        {
            0 => [english],
            1 => [german],
            _ => [english, german]
        };
    }
}