using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniList.Data;
using MediaBrowser.Plugins.AniMetadata.AniList.Requests;
using MediaBrowser.Plugins.AniMetadata.JsonApi;

namespace MediaBrowser.Plugins.AniMetadata.AniList
{
    internal interface IAniListClient
    {
        OptionAsync<Either<FailedRequest, Response<AniListQueryResponse<AniListGraphQlPage<FindSeriesRequest.FindSeriesResponse>>>>>
            FindSeriesAsync(string title);
    }

    internal class AniListClient : IAniListClient
    {
        private readonly IJsonConnection _jsonConnection;
        private readonly IAniListToken _aniListToken;

        public AniListClient(IJsonConnection jsonConnection, IAniListToken aniListToken)
        {
            _jsonConnection = jsonConnection;
            _aniListToken = aniListToken;
        }

        public OptionAsync<Either<FailedRequest,
            Response<AniListQueryResponse<AniListGraphQlPage<FindSeriesRequest.FindSeriesResponse>>>>> FindSeriesAsync(
            string title)
        {
            var token = _aniListToken.GetToken();

            var request = new FindSeriesRequest(title);

            return token.Map(t =>  _jsonConnection.PostAsync(request, t));
        }
    }

    internal interface IAnilistConfiguration
    {
        bool IsLinked { get; }

        string AuthorisationCode { get; }
    }
}