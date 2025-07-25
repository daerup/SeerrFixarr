using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace SeerrFixarr.WireMock.Infrastructure;

public static class LottieWireMockExtensions
{
    public static void MockLottieRequest(this WireMockServer server, string lottieName)
    {
        var lottiePath = $"/lottie/{lottieName}.json";
        var lottieFile = $"Lottie/{lottieName}.json";
        var request = Request.Create().WithPath(lottiePath);
        server.Given(request.UsingGet())
              .RespondWith(Response.Create()
                                   .WithStatusCode(200)
                                   .WithBodyFromFile(lottieFile)
                                   .WithHeader("Content-Type", "application/json"));
    }
}