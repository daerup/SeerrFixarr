using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace SeerrFixarr.Test;

public class ArchitectureTests
{
    [Fact]
    public Task Run() => VerifyChecks.Run();
}