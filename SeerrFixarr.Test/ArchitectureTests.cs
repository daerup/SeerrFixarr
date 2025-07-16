using System.Reflection;
using NetArchTest.Rules;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.Shared.Settings;

namespace SeerrFixarr.Test;

public class ArchitectureTests
{
    private Types SeerrFixarrTypes { get; }
    
    public ArchitectureTests()
    {
        Type[] type = [typeof(Program), typeof(IOverseerrApi), typeof(SeerrFixarrSettings), GetType()];
        SeerrFixarrTypes = Types.InAssemblies(type.Select(t => Assembly.GetAssembly(t)!).ToArray());
    }

    [Fact]
    public Task CheckVerifyConventions() => VerifyChecks.Run();

    [Fact]
    public void InterfacesHaveIPrefix()
    {
        SeerrFixarrTypes
             .That()
             .AreInterfaces()
             .Should()
             .HaveNameStartingWith("I")
             .Assert();
    }

    [Fact]
    public void AsyncMethodsHaveAsyncSuffix()
    {
        SeerrFixarrTypes
             .That()
             .ResideInNamespaceStartingWith(nameof(SeerrFixarr))
             .And()
             .DoNotResideInNamespaceStartingWith($"{nameof(SeerrFixarr)}.{nameof(Test)}")
             .Should()
             .MeetCustomRule(AsyncSuffixForMethodsRule.ForAsyncMethods())
             .Assert();
    }

    [Fact]
    public void SyncMethodsDoNotHaveAsyncSuffix()
    {
        SeerrFixarrTypes
             .That()
             .ResideInNamespaceStartingWith(nameof(SeerrFixarr))
             .Should()
             .MeetCustomRule(AsyncSuffixForMethodsRule.ForSyncMethods())
             .Assert();
    }
}