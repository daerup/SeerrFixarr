using FakeItEasy;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.Api.Shared;
using SeerrFixarr.WireMock.Infrastructure;
using WireMock.Logging;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;
using WireMock.Types;

namespace SeerrFixarr.WireMock;

public class WireMockProgram
{
    public static void Main(string[] args)
    {
        var server = WireMockServer.Start(new WireMockServerSettings
        {
            CorsPolicyOptions = CorsPolicyOptions.AllowAll,
            Port = WireMockProgramHealth.Port,
            Logger = new WireMockConsoleLogger(),
            StartAdminInterface = true,
        });

        server.ForMethod(overseerr => overseerr.PostIssueComment(A<int>._, A<Comment>._)).ReturnsSuccess();
        server.ForMethod(overseerr => overseerr.GetIssue(53)).ReturnsSuccess("overseerr/movieIssue.json");
        server.ForMethod(overseerr => overseerr.GetIssue(55)).ReturnsSuccess("overseerr/episodeIssue.json");
        server.ForMethod(overseerr => overseerr.GetLocalSettingsOfUser(1)).ReturnsSuccess("overseerr/mockedUserSettings.json");

        server.ForMethod(sonarr => sonarr.GetEpisodes(57, 23, true)).ReturnsSuccess("sonarr/57S23.json");
        server.ForMethod(sonarr => sonarr.GetEpisodeReleases(7071)).ReturnsSuccess("sonarr/57S23E09Releases.json");
        server.ForMethod(sonarr => sonarr.InteractiveGrabEpisode(A<InteractiveReleaseGrabRequest>._)).ReturnsSuccess();

        server.MockLottieRequest("amongUs");
        server.MockLottieRequest("dino");
        server.MockLottieRequest("success");

        Console.WriteLine("Press any key to stop...");
        Console.ReadKey();

        server.Stop();
    }
}