using SeerrFixarr.Api.Lottie;

namespace SeerrFixarr.WireMock.Lottie;

public class WireMockLottieProvider : ILottieProvider
{
    public string GetAmongUsLottieUrl() => "http://localhost:9090/lottie/amongUs.json";
    public string GetDinoLottieUrl() => "http://localhost:9090/lottie/dino.json";
    public string GetSuccessLottieUrl() => "http://localhost:9090/lottie/success.json";
}