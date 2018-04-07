using System;
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
        private readonly IAnilistConfiguration _configuration;
        private readonly IJsonConnection _jsonConnection;
        private readonly AniListToken _token;

        public AniListClient(IJsonConnection jsonConnection, IAnilistConfiguration configuration)
        {
            _jsonConnection = jsonConnection;
            _configuration = configuration;

            _token = new AniListToken(jsonConnection, configuration);
        }

        public OptionAsync<AniListSeriesData> FindSeriesAsync(string title)
        {
            throw new NotImplementedException();
        }
    }

    internal interface IAnilistConfiguration
    {
        bool IsLinked { get; }

        string AuthorisationCode { get; }
    }
}