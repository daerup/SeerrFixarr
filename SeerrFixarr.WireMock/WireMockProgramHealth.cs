namespace SeerrFixarr.WireMock;

internal record WireMockProgramHealth
{
    private WireMockProgramHealth(bool IsRunning) => this.IsRunning = IsRunning;

    public const string Endpoint = "/isRunning";
    public const int Port = 9090;
    public bool IsRunning { get; }
    public static WireMockProgramHealth Running() => new WireMockProgramHealth(true);
}