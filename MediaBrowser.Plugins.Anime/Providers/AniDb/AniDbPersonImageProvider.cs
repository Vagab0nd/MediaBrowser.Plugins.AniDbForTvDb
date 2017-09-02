using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Functional.Maybe;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using MediaBrowser.Plugins.Anime.AniDb;

namespace MediaBrowser.Plugins.Anime.Providers.AniDb
{
    public class AniDbPersonImageProvider : IRemoteImageProvider
    {
        private readonly IAniDbClient _aniDbClient;
        private readonly IHttpClient _httpClient;
        private readonly IRateLimiter _rateLimiter;

        public AniDbPersonImageProvider(IAniDbClient aniDbClient, IRateLimiters rateLimiters, IHttpClient httpClient)
        {
            _aniDbClient = aniDbClient;
            _rateLimiter = rateLimiters.AniDb;
            _httpClient = httpClient;
        }

        public bool Supports(IHasMetadata item)
        {
            return item is Person;
        }

        public string Name => "AniDB";

        public IEnumerable<ImageType> GetSupportedImages(IHasMetadata item)
        {
            yield return ImageType.Primary;
        }

        public Task<IEnumerable<RemoteImageInfo>> GetImages(IHasMetadata item, CancellationToken cancellationToken)
        {
            var result = Enumerable.Empty<RemoteImageInfo>();

            var personId =
                MaybeFunctionalWrappers.Wrap<string, int>(int.TryParse)(
                    item.ProviderIds.GetOrDefault(ProviderNames.AniDb));

            personId.Do(id => _aniDbClient.GetSeiyuu(id)
                .Do(s => result = new[]
                {
                    new RemoteImageInfo
                    {
                        ProviderName = ProviderNames.AniDb,
                        Type = ImageType.Primary,
                        Url = s.PictureUrl
                    }
                }));

            return Task.FromResult(result);
        }

        public async Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            await _rateLimiter.TickAsync().ConfigureAwait(false);

            return await _httpClient.GetResponse(new HttpRequestOptions
                {
                    CancellationToken = cancellationToken,
                    Url = url,
                    ResourcePool = _rateLimiter.Semaphore
                })
                .ConfigureAwait(false);
        }
    }
}