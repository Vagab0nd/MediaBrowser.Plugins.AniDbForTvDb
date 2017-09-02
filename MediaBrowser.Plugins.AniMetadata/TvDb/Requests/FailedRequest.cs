using System.Net;

namespace MediaBrowser.Plugins.AniMetadata.TvDb.Requests
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
    }
}