using System.Text;

namespace SeerrFixarr.Api.GeneratedClients;

internal record GeneratedClientOptions(string Host, string ApiKey, IHttpClientFactory factory);

internal class GeneratedClientBase(GeneratedClientOptions options)
{
    public string BaseUrl { get; set; }
    protected Task<HttpClient> CreateHttpClientAsync(CancellationToken cancellationToken)
    {
        var client = options.factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", options.ApiKey);
        client.BaseAddress = new Uri(options.Host);
        return Task.FromResult(client);
    }
    
}
public class ApiException : Exception
{
    public int StatusCode { get; private set; }

    public string Response { get; private set; }

    public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; private set; }

    internal ApiException(string message, int statusCode, string response, IReadOnlyDictionary<string, IEnumerable<string>> headers, Exception innerException)
        : base(TransformMessage(message, statusCode, response), innerException)
    {
        StatusCode = statusCode;
        Response = response;
        Headers = headers;
    }

    public override string ToString()
    {
        var responseBuilder = new StringBuilder();
        responseBuilder.AppendLine("HTTP Response:");
        responseBuilder.AppendLine();
        responseBuilder.AppendLine(Response);
        responseBuilder.AppendLine();
        responseBuilder.AppendLine(base.ToString());
        return responseBuilder.ToString();
    }
    
    private static string TransformMessage(string message, int statusCode, string? response)
    {
        short maxLength = 512;
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(message);
        stringBuilder.AppendLine("Status code: " + statusCode);
        stringBuilder.AppendLine("Response: ");
        if (response is null) stringBuilder.Append("(null)"); 
        else stringBuilder.Append(response, 0, Math.Min(response.Length, maxLength));
        return stringBuilder.ToString();
    }
}