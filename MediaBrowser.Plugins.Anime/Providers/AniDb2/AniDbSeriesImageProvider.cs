using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Functional.Maybe;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using MediaBrowser.Plugins.Anime.AniDb;

namespace MediaBrowser.Plugins.Anime.Providers.AniDb2
{
    public class AniDbSeriesImageProvider : IRemoteImageProvider
    {
        private readonly IAniDbClient _aniDbClient;
        private readonly IHttpClient _httpClient;
        private readonly IRateLimiter _rateLimiter;
        private readonly ILogger _log;

        public AniDbSeriesImageProvider(IAniDbClient aniDbClient, IRateLimiters rateLimiters, IHttpClient httpClient, ILogManager logManager)
        {
            _aniDbClient = aniDbClient;
            _httpClient = httpClient;
            _rateLimiter = rateLimiters.AniDb;
            _log = logManager.GetLogger(nameof(AniDbSeriesImageProvider));
        }

        public bool Supports(IHasImages item)
        {
            return item is Series;
        }

        public string Name => "AniDB";

        public IEnumerable<ImageType> GetSupportedImages(IHasImages item)
        {
            return new[] { ImageType.Primary };
        }

        public async Task<IEnumerable<RemoteImageInfo>> GetImages(IHasImages item, CancellationToken cancellationToken)
        {
            var aniDbSeries =
                await _aniDbClient.GetSeriesAsync(item.ProviderIds.GetOrDefault(ProviderNames.AniDb));
            var imageInfos = new List<RemoteImageInfo>();

            aniDbSeries.Match(s =>
            {
                var imageUrl = GetImageUrl(s.PictureFileName);

                imageUrl.Match(url =>
                {
                    _log.Debug($"Adding series image: {url}");

                    imageInfos.Add(new RemoteImageInfo
                    {
                        ProviderName = ProviderNames.AniDb,
                        Url = url
                    });
                },
                () => _log.Debug($"No image Url specified for '{item.Name}'"));
            },
            () => _log.Debug($"Failed to find AniDb series for '{item.Name}'"));

            
            return imageInfos;
        }

        public async Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            await _rateLimiter.TickAsync().ConfigureAwait(false);

            return await _httpClient.GetResponse(new HttpRequestOptions
            {
                CancellationToken = cancellationToken,
                Url = url,
                ResourcePool = _rateLimiter.Semaphore
            }).ConfigureAwait(false);
        }

        private Maybe<string> GetImageUrl(string imageFileName)
        {
            var result = Maybe<string>.Nothing;

            if (!string.IsNullOrWhiteSpace(imageFileName))
            {
                result = $"http://img7.anidb.net/pics/anime/{imageFileName}".ToMaybe();
            }

            return result;
        }
    }
}