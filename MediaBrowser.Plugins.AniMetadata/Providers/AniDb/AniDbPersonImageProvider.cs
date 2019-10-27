using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emby.AniDbMetaStructure.AniDb;
using Emby.AniDbMetaStructure.Infrastructure;
using Emby.AniDbMetaStructure.Process.Sources;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using static LanguageExt.Prelude;

namespace Emby.AniDbMetaStructure.Providers.AniDb
{
    public class AniDbPersonImageProvider
    {
        private readonly IAniDbClient aniDbClient;
        private readonly IHttpClient httpClient;
        private readonly IRateLimiter rateLimiter;

        public AniDbPersonImageProvider(IAniDbClient aniDbClient, IRateLimiters rateLimiters, IHttpClient httpClient)
        {
            this.aniDbClient = aniDbClient;
            this.rateLimiter = rateLimiters.AniDb;
            this.httpClient = httpClient;
        }

        public bool Supports(BaseItem item)
        {
            return item is Person;
        }

        public string Name => SourceNames.AniDb;

        public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
        {
            yield return ImageType.Primary;
        }

        public Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
        {
            var result = Enumerable.Empty<RemoteImageInfo>();

            var personId =
                parseInt(item.ProviderIds.GetOrDefault(SourceNames.AniDb));

            personId.Iter(id => this.aniDbClient.GetSeiyuu(id)
                .Iter(s => result = new[]
                {
                    new RemoteImageInfo
                    {
                        ProviderName = SourceNames.AniDb,
                        Type = ImageType.Primary,
                        Url = s.PictureUrl
                    }
                }));

            return Task.FromResult(result);
        }

        public async Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            await this.rateLimiter.TickAsync().ConfigureAwait(false);

            return await this.httpClient.GetResponse(new HttpRequestOptions
                {
                    CancellationToken = cancellationToken,
                    Url = url
                })
                .ConfigureAwait(false);
        }
    }
}