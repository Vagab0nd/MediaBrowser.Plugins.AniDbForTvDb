using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using MediaBrowser.Plugins.Anime.Providers.AniDB.Converter;

namespace MediaBrowser.Plugins.Anime.Providers.AniDB.Metadata
{
    public class AniDbSeasonProvider //: IRemoteMetadataProvider<Season, SeasonInfo>
    {
        private readonly AnidbConverter _anidbConverter;
        private readonly ILogger _log;
        private readonly AniDbSeriesProvider _seriesProvider;

        public AniDbSeasonProvider(IHttpClient httpClient, IApplicationPaths appPaths, ILogManager logManager)
        {
            _seriesProvider = new AniDbSeriesProvider(appPaths, httpClient, logManager);
            _log = logManager.GetLogger(nameof(AniDbSeasonProvider));
            _anidbConverter = new AnidbConverter(appPaths, logManager);
        }

        public async Task<MetadataResult<Season>> GetMetadata(SeasonInfo info, CancellationToken cancellationToken)
        {
            _log.Debug($"{nameof(GetMetadata)}: info '{info.Name}'");

            var result = new MetadataResult<Season>
            {
                HasMetadata = true,
                Item = new Season
                {
                    Name = info.Name,
                    IndexNumber = info.IndexNumber
                }
            };

            var anidbSeriesId = info.SeriesProviderIds.GetOrDefault(ProviderNames.AniDb);
            if (anidbSeriesId == null)
            {
                _log.Debug($"{nameof(GetMetadata)}: No AniDb seriesId found on parent series");
                return result;
            }

            var seriesInfo = new SeriesInfo();
            seriesInfo.ProviderIds.Add(ProviderNames.AniDb, anidbSeriesId);

            var seriesResult = await _seriesProvider.GetMetadata(seriesInfo, cancellationToken);
            if (seriesResult.HasMetadata)
            {
                result.Item.Name = seriesResult.Item.Name;
                result.Item.Overview = seriesResult.Item.Overview;
                result.Item.PremiereDate = seriesResult.Item.PremiereDate;
                result.Item.EndDate = seriesResult.Item.EndDate;
                result.Item.CommunityRating = seriesResult.Item.CommunityRating;
                //result.Item.VoteCount = seriesResult.Item.VoteCount;
                result.Item.Studios = seriesResult.Item.Studios;
                result.Item.Genres = seriesResult.Item.Genres;
            }
            else
            {
                _log.Debug($"{nameof(GetMetadata)}: No series metadata found");
            }

            _log.Debug(
                $"{nameof(GetMetadata)}: Found metadata '{result.Item.Name}' season number {result.Item.IndexNumber}");

            return result;
        }

        public string Name => "AniDB";

        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeasonInfo searchInfo,
            CancellationToken cancellationToken)
        {
            var metadata = await GetMetadata(searchInfo, cancellationToken).ConfigureAwait(false);

            var list = new List<RemoteSearchResult>();

            if (metadata.HasMetadata)
            {
                var res = new RemoteSearchResult
                {
                    Name = metadata.Item.Name,
                    PremiereDate = metadata.Item.PremiereDate,
                    ProductionYear = metadata.Item.ProductionYear,
                    ProviderIds = metadata.Item.ProviderIds,
                    SearchProviderName = Name
                };

                list.Add(res);
            }

            return list;
        }

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}