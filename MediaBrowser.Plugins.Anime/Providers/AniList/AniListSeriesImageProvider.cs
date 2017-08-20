using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using MediaBrowser.Plugins.Anime.Providers.AniDB.Metadata;

namespace MediaBrowser.Plugins.Anime.Providers.AniList
{
    public class AniListSeriesImageProvider : IRemoteImageProvider
    {
        private readonly IApplicationPaths _appPaths;
        private readonly IHttpClient _httpClient;

        public AniListSeriesImageProvider(IHttpClient httpClient, IApplicationPaths appPaths)
        {
            _httpClient = httpClient;
            _appPaths = appPaths;
        }

        public string Name => "AniList";

        public bool Supports(IHasMetadata item)
        {
            return item is Series || item is Season;
        }

        public IEnumerable<ImageType> GetSupportedImages(IHasMetadata item)
        {
            return new[] { ImageType.Primary, ImageType.Banner };
        }

        public Task<IEnumerable<RemoteImageInfo>> GetImages(IHasMetadata item, CancellationToken cancellationToken)
        {
            var seriesId = item.GetProviderId(ProviderNames.AniList);
            return GetImages(seriesId, cancellationToken);
        }

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return _httpClient.GetResponse(new HttpRequestOptions
            {
                CancellationToken = cancellationToken,
                Url = url,
                ResourcePool = AniDbSeriesProvider.ResourcePool
            });
        }

        public async Task<IEnumerable<RemoteImageInfo>> GetImages(string aid, CancellationToken cancellationToken)
        {
            var list = new List<RemoteImageInfo>();

            if (!string.IsNullOrEmpty(aid))
            {
                var primary = AniListSeriesProvider.GetSeriesImage(_appPaths, aid, "image");
                if (!string.IsNullOrEmpty(primary))
                {
                    list.Add(new RemoteImageInfo
                    {
                        ProviderName = Name,
                        Type = ImageType.Primary,
                        Url = primary
                    });
                }

                var banner = AniListSeriesProvider.GetSeriesImage(_appPaths, aid, "banner");
                if (!string.IsNullOrEmpty(banner))
                {
                    list.Add(new RemoteImageInfo
                    {
                        ProviderName = Name,
                        Type = ImageType.Banner,
                        Url = banner
                    });
                }
            }

            return list;
        }
    }
}