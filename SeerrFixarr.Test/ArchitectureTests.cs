using System.Runtime.CompilerServices;

namespace SeerrFixarr.Test;

public class ArchitectureTests
{
    [Fact]
    public Task Run() => VerifyChecks.Run();
}

public static class StaticSettingsUsage
{
    [ModuleInitializer]
    public static void Initialize()
    {
        // Set custom directory for all tests in the assembly
        var settings = new VerifySettings();
        settings.UseDirectory("SeerrFixarr.Test");
    }
}