using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniList.Data;
using MediaBrowser.Plugins.AniMetadata.AniList.Requests;
using MediaBrowser.Plugins.AniMetadata.JsonApi;
using MediaBrowser.Plugins.AniMetadata.Process;

namespace MediaBrowser.Plugins.AniMetadata.AniList
{
    internal class AniListClient : IAniListClient
    {
        private readonly IAniListToken _aniListToken;
        private readonly IJsonConnection _jsonConnection;

        public AniListClient(IJsonConnection jsonConnection, IAniListToken aniListToken)
        {
            _jsonConnection = jsonConnection;
            _aniListToken = aniListToken;
        }

        public Task<Either<ProcessFailedResult, IEnumerable<AniListSeriesData>>> FindSeriesAsync(string title,
            ProcessResultContext resultContext)
        {
            var token = _aniListToken.GetToken(resultContext);

            var request = new FindSeriesRequest(title);

            return token.BindAsync(t =>
            {
                return _jsonConnection.PostAsync(request, t)
                    .MapAsync(r => r.Data.Data.Page.Media)
                    .Map(e => e.MapLeft(FailedRequest.ToFailedResult(resultContext)));
            });
        }
    }
}