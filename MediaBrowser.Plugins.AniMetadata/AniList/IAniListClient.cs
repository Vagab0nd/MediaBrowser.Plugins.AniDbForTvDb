using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniList.Data;
using MediaBrowser.Plugins.AniMetadata.JsonApi;

namespace MediaBrowser.Plugins.AniMetadata.AniList
{
    internal interface IAniListClient
    {
        OptionAsync<AniListSeriesData> FindSeriesAsync(string title);
    }

    internal class AniListClient : IAniListClient
    {
        private readonly IJsonConnection _jsonConnection;

        public AniListClient(IJsonConnection jsonConnection)
        {
            _jsonConnection = jsonConnection;
        }

        public OptionAsync<AniListSeriesData> FindSeriesAsync(string title)
        {
            throw new System.NotImplementedException();
        }
    }
}