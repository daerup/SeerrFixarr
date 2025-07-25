using System.Linq.Expressions;
using SeerrFixarr.Api.Overseerr;
using SeerrFixarr.Api.Radarr;
using SeerrFixarr.Api.Sonarr;
using WireMock.Server;

namespace SeerrFixarr.WireMock.Infrastructure;

public static class RefitWireMockExtensions
{
    public static RefitMethodStubBuilder ForMethod(
        this WireMockServer server,
        Expression<Func<IOverseerrApi, object>> expression)
    {
        const string overseerrApi = "/overseerr/api";
        return ForMethod(server, expression, overseerrApi);
    }

    public static RefitMethodStubBuilder ForMethod(
        this WireMockServer server,
        Expression<Func<ISonarrApi, object>> expression)
    {
        const string sonarrApi = "/sonarr/api";
        return ForMethod(server, expression, sonarrApi);
    }

    public static RefitMethodStubBuilder ForMethod(
        this WireMockServer server,
        Expression<Func<IRadarrApi, object>> expression)
    {
        const string radarrApi = "/radarr/api";
        return ForMethod(server, expression, radarrApi);
    }

    private static RefitMethodStubBuilder ForMethod<T>(
        WireMockServer server,
        Expression<Func<T, object>> expression, string host)
    {
        if (expression.Body is not MethodCallExpression methodCall)
        {
            throw new ArgumentException("Expected a method call expression");
        }

        var method = methodCall.Method;
        var arguments = methodCall.Arguments;

        return new RefitMethodStubBuilder(server, method, arguments, host);
    }
}