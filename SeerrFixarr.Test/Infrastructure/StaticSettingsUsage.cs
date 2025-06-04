using System.Runtime.CompilerServices;

namespace SeerrFixarr.Test.Infrastructure;

public static class StaticSettingsUsage
{
    [ModuleInitializer]
    public static void Initialize()
    {
        UseProjectRelativeDirectory(".verify");
    }
}