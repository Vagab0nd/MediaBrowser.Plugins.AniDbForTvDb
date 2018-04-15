using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniList.Requests;
using MediaBrowser.Plugins.AniMetadata.JsonApi;
using MediaBrowser.Plugins.AniMetadata.Process;

namespace MediaBrowser.Plugins.AniMetadata.AniList
{
    internal class AniListToken : IAniListToken
    {
        private readonly IAnilistConfiguration _anilistConfiguration;
        private readonly IJsonConnection _jsonConnection;

        public AniListToken(IJsonConnection jsonConnection, IAnilistConfiguration anilistConfiguration)
        {
            _jsonConnection = jsonConnection;
            _anilistConfiguration = anilistConfiguration;
        }

        public Task<Either<ProcessFailedResult, string>> GetToken(ProcessResultContext resultContext)
        {
            var response = _jsonConnection
                .PostAsync(new GetTokenRequest(362, "NSjmeTEekFlV9OZuZo9iR0BERNe3KS83iaIiI7EQ",
                    "http://localhost:8096/web/configurationpage?name=AniMetadata",
                    _anilistConfiguration.AuthorisationCode), Option<string>.None);

            var token = response.MapAsync(r => r.Data.AccessToken)
                .Map(e => e.MapLeft(FailedRequest.ToFailedResult(resultContext)));

            return token;
        }
    }
}