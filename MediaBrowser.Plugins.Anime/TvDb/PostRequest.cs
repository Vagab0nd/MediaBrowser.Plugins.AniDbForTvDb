using System.IO;

namespace MediaBrowser.Plugins.Anime.TvDb
{
    internal abstract class PostRequest<TResponse>
    {
        private const string TvDbBaseUrl = "api.thetvdb.com/";
        private readonly string _urlPath;

        public PostRequest(string urlPath, object data)
        {
            _urlPath = urlPath;
            Data = data;
        }

        public object Data { get; }

        public string Url => Path.Combine(TvDbBaseUrl, _urlPath);
    }
}