using System;
using System.Net;
using MediaBrowser.Plugins.AniMetadata.Process;

namespace MediaBrowser.Plugins.AniMetadata.JsonApi
{
    internal class FailedRequest
    {
        public FailedRequest(HttpStatusCode statusCode, string responseContent)
        {
            StatusCode = statusCode;
            ResponseContent = responseContent;
        }

        public HttpStatusCode StatusCode { get; }

        public string ResponseContent { get; }

        public static Func<FailedRequest, ProcessFailedResult> ToFailedResult(ProcessResultContext resultContext)
        {
            return r => resultContext.Failed($"Request failed with {r.StatusCode}: {r.ResponseContent}");
        }
    }
}