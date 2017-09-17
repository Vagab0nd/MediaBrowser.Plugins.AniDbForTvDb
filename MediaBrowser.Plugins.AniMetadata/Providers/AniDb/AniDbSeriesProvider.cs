using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.Providers.AniDb
{
    public class AniDbSeriesProvider : IRemoteMetadataProvider<Series, SeriesInfo>, IHasOrder
    {
        private readonly ISeriesDataLoader _seriesDataLoader;
        private readonly ILogger _log;
        private readonly ISeriesMetadataFactory _seriesMetadataFactory;

        public AniDbSeriesProvider(ILogManager logManager, ISeriesMetadataFactory seriesMetadataFactory,
            ISeriesDataLoader seriesDataLoader)
        {
            _seriesMetadataFactory = seriesMetadataFactory;
            _seriesDataLoader = seriesDataLoader;
            _log = logManager.GetLogger(nameof(AniDbSeriesProvider));
        }

        public int Order => -1;

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeriesInfo searchInfo,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<MetadataResult<Series>> GetMetadata(SeriesInfo info, CancellationToken cancellationToken)
        {
            return _seriesDataLoader.GetSeriesDataAsync(info)
                .Map(d => d.Match(
                    data => SetProviderIds(GetAniDbMetadata(info, data.AniDbSeriesData), data.SeriesIds),
                    combinedData =>
                        SetProviderIds(
                            GetCombinedMetadata(info, combinedData.AniDbSeriesData, combinedData.TvDbSeriesData),
                            combinedData.SeriesIds),
                    noData => _seriesMetadataFactory.NullSeriesResult));
        }

        public string Name => ProviderNames.AniDb;

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        private MetadataResult<Series> GetAniDbMetadata(SeriesInfo info,
            AniDbSeriesData aniDbSeriesData)
        {
            return _seriesMetadataFactory.CreateMetadata(aniDbSeriesData, info.MetadataLanguage);
        }

        private MetadataResult<Series> GetCombinedMetadata(SeriesInfo info, AniDbSeriesData aniDbSeriesData,
            TvDbSeriesData tvDbSeriesData)
        {
            return _seriesMetadataFactory.CreateMetadata(aniDbSeriesData, tvDbSeriesData, info.MetadataLanguage);
        }

        private MetadataResult<Series> SetProviderIds(MetadataResult<Series> metadata, SeriesIds seriesIds)
        {
            metadata.Item.SetProviderId(ProviderNames.AniDb, seriesIds.AniDbSeriesId.ToString());

            seriesIds.TvDbSeriesId.Iter(id =>
            {
                metadata.Item.SetProviderId(MetadataProviders.Tvdb, id.ToString());
                _log.Debug($"Found TvDb Id: {id}");
            });

            seriesIds.ImdbSeriesId.Iter(id =>
            {
                metadata.Item.SetProviderId(MetadataProviders.Imdb, id.ToString());
                _log.Debug($"Found Imdb Id: {id}");
            });

            seriesIds.TmDbSeriesId.Iter(id =>
            {
                metadata.Item.SetProviderId(MetadataProviders.Tmdb, id.ToString());
                _log.Debug($"Found TmDb Id: {id}");
            });

            return metadata;
        }
    }
}