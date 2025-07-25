using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace SeerrFixarr.WireMock.Infrastructure;

public class RefitMethodStubBuilder(
    WireMockServer server,
    MethodInfo method,
    IReadOnlyList<Expression> arguments,
    string host)
{
    public void ReturnsSuccess()
    {
        var path = GetPathFromAttribute(host);
        var request = CreateRequestBuilder(path);
        server
            .Given(request)
            .RespondWith(Response.Create()
                                 .WithStatusCode(200)
                                 .WithRandomDelay(100, 200));
    }

    public void ReturnsSuccess(string jsonFile)
    {
        var path = GetPathFromAttribute(host);
        var request = CreateRequestBuilder(path);
        server
            .Given(request)
            .RespondWith(Response.Create()
                                 .WithStatusCode(200)
                                 .WithBodyFromFile(jsonFile)
                                 .WithHeader("Content-Type", "application/json")
                                 .WithRandomDelay(100, 5000));
    }

    public void ReturnsFailure(string jsonFile)
    {
        var path = GetPathFromAttribute(host);
        var request = CreateRequestBuilder(path);
        server
            .Given(request)
            .RespondWith(Response.Create().WithBodyFromFile(jsonFile).WithStatusCode(500));
    }

    private IRequestBuilder GetPathFromAttribute(string s)
    {
        if (method.GetCustomAttribute<Refit.HttpMethodAttribute>() is { } methodAttribute)
        {
            return ReplaceParameters(host, methodAttribute.Path);
        }

        throw new InvalidOperationException("No HTTP method attribute found on method.");
    }

    private IRequestBuilder ReplaceParameters(string host, string pathTemplate)
    {
        var parameters = method.GetParameters();
        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        for (int i = 0; i < parameters.Length && i < arguments.Count; i++)
        {
            var param = parameters[i];
            var name = GetParametername(param);
            var value = GetParameterValue(arguments[i]);
            var token = "{" + name + "}";
            if (pathTemplate.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                pathTemplate = pathTemplate.Replace(token, value);
            }
            else
            {
                if (param.GetCustomAttribute<Refit.BodyAttribute>() is null)
                {
                    queryParams[name] = value;
                }
            }
        }

        var requestBuilder = Request.Create().WithPath(host + pathTemplate);

        foreach (var key in queryParams.AllKeys)
        {
            var value = queryParams[key];
            requestBuilder = requestBuilder.WithParam(key!, value!);
        }

        return requestBuilder;
    }

    private string? GetParametername(ParameterInfo param)
    {
        if (param.GetCustomAttribute<Refit.AliasAsAttribute>() is { } aliasAttribute)
        {
            return aliasAttribute.Name;
        }

        return param.Name;
    }

    private string GetParameterValue(Expression parameterExpression)
    {
        if (IsFakeItEasyAnyMatcher(parameterExpression))
        {
            return "*";
        }

        var value = Expression.Lambda(parameterExpression).Compile().DynamicInvoke();
        return Uri.EscapeDataString(value?.ToString() ?? "");
    }

    private bool IsFakeItEasyAnyMatcher(Expression expression)
    {
        return expression is MemberExpression { Member: PropertyInfo { DeclaringType.Namespace: "FakeItEasy" } };
    }

    private IRequestBuilder CreateRequestBuilder(IRequestBuilder requestBuilder)
    {
        if (method.GetCustomAttribute<Refit.HttpMethodAttribute>() is not { } httpMethod)
        {
            throw new InvalidOperationException("No HTTP method attribute found on method.");
        }

        return httpMethod switch
        {
            Refit.GetAttribute => requestBuilder.UsingGet(),
            Refit.PostAttribute => requestBuilder.UsingPost(),
            Refit.PutAttribute => requestBuilder.UsingPut(),
            Refit.DeleteAttribute => requestBuilder.UsingDelete(),
            _ => throw new NotSupportedException()
        };
    }
}