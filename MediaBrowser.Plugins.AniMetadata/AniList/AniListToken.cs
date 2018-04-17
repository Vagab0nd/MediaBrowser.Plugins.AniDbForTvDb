using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniList.Requests;
using MediaBrowser.Plugins.AniMetadata.JsonApi;
using MediaBrowser.Plugins.AniMetadata.Process;
using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata.AniList
{
    internal class AniListToken : IAniListToken
    {
        private readonly ConcurrentDictionary<string, Task<Either<FailedRequest, string>>> _accessTokens =
            new ConcurrentDictionary<string, Task<Either<FailedRequest, string>>>();

        public Task<Either<FailedRequest, string>> GetToken(IJsonConnection jsonConnection,
            IAnilistConfiguration anilistConfiguration, ProcessResultContext resultContext)
        {
            return anilistConfiguration.AccessToken.MapAsync(Right<FailedRequest, string>)
                .IfNone(() => GetTokenFromCacheOrRequest(jsonConnection, anilistConfiguration));
        }

        private Task<Either<FailedRequest, string>> GetTokenFromCacheOrRequest(IJsonConnection jsonConnection,
            IAnilistConfiguration anilistConfiguration)
        {
            return GetCachedToken(anilistConfiguration.AuthorisationCode,
                () => RequestToken(jsonConnection, anilistConfiguration));
        }

        private Task<Either<FailedRequest, string>> GetCachedToken(string authorisationCode,
            Func<Task<Either<FailedRequest, string>>> requestToken)
        {
            var token = _accessTokens.GetOrAdd(authorisationCode, k => requestToken());

            return token;
        }

        private Task<Either<FailedRequest, string>> RequestToken(IJsonConnection jsonConnection,
            IAnilistConfiguration anilistConfiguration)
        {
            var response = jsonConnection
                .PostAsync(new GetTokenRequest(362, "NSjmeTEekFlV9OZuZo9iR0BERNe3KS83iaIiI7EQ",
                    "http://localhost:8096/web/configurationpage?name=AniMetadata",
                    anilistConfiguration.AuthorisationCode), Option<string>.None);

            var token = response.MapAsync(r =>
            {
                anilistConfiguration.AccessToken = r.Data.AccessToken;
                return r.Data.AccessToken;
            });

            return token;
        }
    }
}