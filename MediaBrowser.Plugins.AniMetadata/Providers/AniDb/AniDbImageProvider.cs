using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;

namespace MediaBrowser.Plugins.AniMetadata.Providers.AniDb
{
    public class AniDbImageProvider
    {
        private readonly IAniDbClient _aniDbClient;
        private readonly IHttpClient _httpClient;
        private readonly ILogger _log;
        private readonly IRateLimiter _rateLimiter;

        public AniDbImageProvider(IAniDbClient aniDbClient, IRateLimiters rateLimiters, IHttpClient httpClient,
            ILogManager logManager)
        {
            _aniDbClient = aniDbClient;
            _httpClient = httpClient;
            _rateLimiter = rateLimiters.AniDb;
            _log = logManager.GetLogger(nameof(AniDbImageProvider));
        }

        public bool Supports(BaseItem item)
        {
            return item is Series || item is Season;
        }

        public string Name => SourceNames.AniDb;

        public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
        {
            return new[] { ImageType.Primary };
        }

        public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item,
            CancellationToken cancellationToken)
        {
            var imageInfos = new List<RemoteImageInfo>();

            var embySeries = GetEmbySeries(item);

            var aniDbSeries =
                await embySeries.Match(GetAniDbSeriesAsync, () => Task.FromResult(Option<AniDbSeriesData>.None));

            aniDbSeries
                .Match(s =>
                    {
                        var imageUrl = GetImageUrl(s.PictureFileName);

                        imageUrl.Match(url =>
                            {
                                _log.Debug($"Adding series image: {url}");

                                imageInfos.Add(new RemoteImageInfo
                                {
                                    ProviderName = SourceNames.AniDb,
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
                    Url = url
                })
                .ConfigureAwait(false);
        }

        private Option<Series> GetEmbySeries(BaseItem item)
        {
            return item as Series ?? (item as Season)?.Series;
        }

        private Task<Option<AniDbSeriesData>> GetAniDbSeriesAsync(Series embySeries)
        {
            return _aniDbClient.GetSeriesAsync(embySeries.ProviderIds.GetOrDefault(SourceNames.AniDb));
        }

        private Option<string> GetImageUrl(string imageFileName)
        {
            var result = Option<string>.None;

            if (!string.IsNullOrWhiteSpace(imageFileName))
            {
                result = $"http://img7.anidb.net/pics/anime/{imageFileName}";
            }

            return result;
        }
    }
}