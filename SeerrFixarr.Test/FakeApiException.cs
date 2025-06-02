using System;
using System.Net;
using System.Net.Http;
using Refit;

namespace SeerrFixarr.Test;

internal class FakeApiException(Exception e) : ApiException(e.Message, new HttpRequestMessage(), HttpMethod.Post, null,
  HttpStatusCode.InternalServerError, null, new HttpResponseMessage().Headers, null!, e);