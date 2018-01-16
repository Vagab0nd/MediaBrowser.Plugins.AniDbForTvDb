using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata.Providers.AniDb
{
    public class AniDbPersonImageProvider
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

        public bool Supports(BaseItem item)
        {
            return item is Person;
        }

        public string Name => ProviderNames.AniDb;

        public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
        {
            yield return ImageType.Primary;
        }

        public Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
        {
            var result = Enumerable.Empty<RemoteImageInfo>();

            var personId =
                parseInt(item.ProviderIds.GetOrDefault(ProviderNames.AniDb));

            personId.Iter(id => _aniDbClient.GetSeiyuu(id)
                .Iter(s => result = new[]
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