using System.IO;
using MediaBrowser.Plugins.AniMetadata.JsonApi;

namespace MediaBrowser.Plugins.AniMetadata.TvDb.Requests
{
    internal abstract class TvDbRequest<TResponse> : Request<TResponse>
    {
        private const string TvDbBaseUrl = "https://api.thetvdb.com/";

        protected TvDbRequest(string urlPath) : base(Path.Combine(TvDbBaseUrl, urlPath))
        {
        }
    }
}