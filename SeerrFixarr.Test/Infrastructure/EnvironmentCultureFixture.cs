using SeerrFixarr.Test.Infrastructure;

[assembly: AssemblyFixture (typeof(EnvironmentCultureFixture))]

namespace SeerrFixarr.Test.Infrastructure;

public class EnvironmentCultureFixture : IDisposable
{
    public EnvironmentCultureFixture() => Environment.SetEnvironmentVariable("CULTURE", "de-CH");

    public void Dispose() => Environment.SetEnvironmentVariable("CULTURE", null);
}