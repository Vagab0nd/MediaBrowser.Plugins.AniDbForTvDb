using System.IO;

namespace MediaBrowser.Plugins.Anime.TvDb.Requests
{
    internal abstract class Request<TResponse>
    {
        private const string TvDbBaseUrl = "api.thetvdb.com/";
        private readonly string _urlPath;

        public Request(string urlPath)
        {
            _urlPath = urlPath;
        }

        public string Url => Path.Combine(TvDbBaseUrl, _urlPath);
    }
}