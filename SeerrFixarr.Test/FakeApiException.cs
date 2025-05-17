using System.Net;
using Refit;

namespace SeerrFixarr.Test;

internal class FakeApiException : ApiException
{
    public FakeApiException(Exception e) : base(e.Message, new HttpRequestMessage(), HttpMethod.Post, null, HttpStatusCode.InternalServerError, null, new HttpResponseMessage().Headers, null, e)
    {
    }
}