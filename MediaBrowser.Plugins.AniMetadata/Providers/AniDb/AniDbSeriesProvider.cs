using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.Mapping;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.Providers.AniDb
{
    public class AniDbSeriesProvider
    {
        private readonly ILogger _log;
        private readonly IPluginConfiguration _pluginConfiguration;
        private readonly ISeriesDataLoader _seriesDataLoader;
        private readonly ISeriesMetadataFactory _seriesMetadataFactory;

        public AniDbSeriesProvider(ILogManager logManager, ISeriesMetadataFactory seriesMetadataFactory,
            ISeriesDataLoader seriesDataLoader, IPluginConfiguration pluginConfiguration)
        {
            _seriesMetadataFactory = seriesMetadataFactory;
            _seriesDataLoader = seriesDataLoader;
            _pluginConfiguration = pluginConfiguration;
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
            if (_pluginConfiguration.ExcludedSeriesNames.Contains(info.Name, StringComparer.InvariantCultureIgnoreCase))
            {
                if (info.ProviderIds.ContainsKey(ProviderNames.AniDb))
                {
                    info.ProviderIds.Remove(ProviderNames.AniDb);
                }

                _log.Info($"Skipping series '{info.Name}' as it is excluded");
                return Task.FromResult(_seriesMetadataFactory.NullResult);
            }

            _log.Info($"Finding data for series '{info.Name}'");

            var seriesResult = _seriesDataLoader.GetSeriesDataAsync(info.Name)
                .Map(d => d.Match(
                    data => SetProviderIds(GetAniDbMetadata(data.AniDbSeriesData, info.MetadataLanguage),
                        data.SeriesIds),
                    combinedData =>
                        SetProviderIds(
                            GetCombinedMetadata(combinedData.AniDbSeriesData, combinedData.TvDbSeriesData,
                                info.MetadataLanguage),
                            combinedData.SeriesIds),
                    noData => _seriesMetadataFactory.NullResult));

            if (seriesResult.Result.HasMetadata)
            {
                _log.Info($"Found data for matching series: '{seriesResult.Result.Item?.Name}'");

                info.IndexNumber = null;
                info.ParentIndexNumber = null;
                info.Name = "";
                info.ProviderIds = new Dictionary<string, string>();
            }
            else
            {
                _log.Info("Found no matching series");
            }

            return seriesResult;
        }

        public string Name => ProviderNames.AniDb;

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        private MetadataResult<Series> GetAniDbMetadata(AniDbSeriesData aniDbSeriesData, string metadataLanguage)
        {
            return _seriesMetadataFactory.CreateMetadata(aniDbSeriesData, metadataLanguage);
        }

        private MetadataResult<Series> GetCombinedMetadata(AniDbSeriesData aniDbSeriesData,
            TvDbSeriesData tvDbSeriesData, string metadataLanguage)
        {
            return _seriesMetadataFactory.CreateMetadata(aniDbSeriesData, tvDbSeriesData, metadataLanguage);
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