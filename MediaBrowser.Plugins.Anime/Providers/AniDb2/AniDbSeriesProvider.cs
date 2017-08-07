using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Functional.Maybe;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using MediaBrowser.Plugins.Anime.AniDb;
using MediaBrowser.Plugins.Anime.AniDb.Data;

namespace MediaBrowser.Plugins.Anime.Providers.AniDb2
{
    public class AniDbSeriesProvider : IRemoteMetadataProvider<Series, SeriesInfo>, IHasOrder
    {
        private readonly IAniDbClient _aniDbClient;
        private readonly IEmbyMetadataFactory _embyMetadataFactory;
        private readonly ILogger _log;

        public AniDbSeriesProvider(ILogManager logManager, IAniDbClient aniDbClient,
            IEmbyMetadataFactory embyMetadataFactory)
        {
            _aniDbClient = aniDbClient;
            _embyMetadataFactory = embyMetadataFactory;
            _log = logManager.GetLogger(nameof(AniDbSeriesProvider));
        }

        public int Order => -1;

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeriesInfo searchInfo,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<MetadataResult<Series>> GetMetadata(SeriesInfo info, CancellationToken cancellationToken)
        {
            AniDbSeries aniDbSeries = null;

            if (!info.ProviderIds.TryGetValue(ProviderNames.AniDb, out string aniDbSeriesIdString) ||
                !int.TryParse(aniDbSeriesIdString, out int aniDbSeriesId))
            {
                var seriesResult = await _aniDbClient.FindSeriesAsync(info.Name);

                seriesResult.Match(
                    s =>
                    {
                        aniDbSeriesId = s.Id;
                        aniDbSeries = s;
                    },
                    () => _log.Warn($"Failed to find an AniDb match for '{info.Name}'"));
            }
            else
            {
                aniDbSeries = await _aniDbClient.GetSeriesAsync(aniDbSeriesId);
            }

            if (aniDbSeries == null)
            {
                return null;
            }

            var metadataResult = _embyMetadataFactory.CreateSeriesMetadataResult(aniDbSeries, info.MetadataLanguage);

            var mapper = await _aniDbClient.GetMapperAsync();

            var tvDbSeriesIdResult = mapper.GetMappedTvDbSeriesId(aniDbSeries.Id);

            tvDbSeriesIdResult.Match(
                tvDbSeriesId => metadataResult.Item.ProviderIds.Add(ProviderNames.TvDb, tvDbSeriesId.Id.ToString()),
                nonTvDbSeriesId => { },
                unknownSeriesId => { });

            return metadataResult;
        }

        public string Name => "AniDB";

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}