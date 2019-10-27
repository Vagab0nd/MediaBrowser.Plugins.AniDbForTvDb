using System;
using System.Net;
using Emby.AniDbMetaStructure.Process;

namespace Emby.AniDbMetaStructure.JsonApi
{
    internal class FailedRequest
    {
        public FailedRequest(HttpStatusCode statusCode, string responseContent)
        {
            this.StatusCode = statusCode;
            this.ResponseContent = responseContent;
        }

        public HttpStatusCode StatusCode { get; }

        public string ResponseContent { get; }

        public static Func<FailedRequest, ProcessFailedResult> ToFailedResult(ProcessResultContext resultContext)
        {
            return r => resultContext.Failed($"Request failed with {r.StatusCode}: {r.ResponseContent}");
        }
    }
}