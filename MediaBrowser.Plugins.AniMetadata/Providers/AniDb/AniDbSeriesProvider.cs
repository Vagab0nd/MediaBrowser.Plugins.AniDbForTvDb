using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.TvDb;

namespace MediaBrowser.Plugins.AniMetadata.Providers.AniDb
{
    public class AniDbSeriesProvider : IRemoteMetadataProvider<Series, SeriesInfo>, IHasOrder
    {
        private readonly IAniDbClient _aniDbClient;
        private readonly ILogger _log;
        private readonly ISeriesMetadataFactory _seriesMetadataFactory;
        private readonly ITvDbClient _tvDbClient;

        public AniDbSeriesProvider(ILogManager logManager, IAniDbClient aniDbClient, ITvDbClient tvDbClient,
            ISeriesMetadataFactory seriesMetadataFactory)
        {
            _aniDbClient = aniDbClient;
            _tvDbClient = tvDbClient;
            _seriesMetadataFactory = seriesMetadataFactory;
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
            return _aniDbClient.FindSeriesAsync(info.Name)
                .MatchAsync(aniDbSeriesData => _aniDbClient.GetMapperAsync()
                        .MatchAsync(mapper => mapper.GetMappedSeriesIds(aniDbSeriesData.Id)
                                .MatchAsync(
                                    seriesIds =>
                                        GetMetadata(seriesIds.TvDbSeriesId, info, aniDbSeriesData)
                                            .Map(m => SetProviderIds(m, seriesIds)),
                                    () =>
                                    {
                                        _log.Debug("No series Id mappings found, using AniDb data only");
                                        return GetAniDbMetadata(info, aniDbSeriesData);
                                    }),
                            () =>
                            {
                                _log.Debug("Failed to load mapping list, using AniDb data only");
                                return GetAniDbMetadata(info, aniDbSeriesData);
                            }),
                    () =>
                    {
                        _log.Debug("Failed to find AniDb series by name");
                        return _seriesMetadataFactory.NullSeriesResult;
                    });
        }

        public string Name => ProviderNames.AniDb;

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        private Task<MetadataResult<Series>> GetMetadata(Option<int> tvDbSeriesId, SeriesInfo info,
            AniDbSeriesData aniDbSeriesData)
        {
            return tvDbSeriesId.MatchAsync(id => GetCombinedMetadataAsync(id, info, aniDbSeriesData)
                    .Match(m => m,
                        () =>
                        {
                            _log.Debug($"Failed to load TvDb series with Id {tvDbSeriesId}, using AniDb data only");
                            return GetAniDbMetadata(info, aniDbSeriesData);
                        }),
                () =>
                {
                    _log.Debug("No TvDb series Id mapped, using AniDb data only, using AniDb data only");
                    return GetAniDbMetadata(info, aniDbSeriesData);
                });
        }

        private MetadataResult<Series> GetAniDbMetadata(SeriesInfo info,
            AniDbSeriesData aniDbSeriesData)
        {
            return _seriesMetadataFactory.CreateMetadata(aniDbSeriesData, info.MetadataLanguage);
        }

        private OptionAsync<MetadataResult<Series>> GetCombinedMetadataAsync(int tvDbSeriesId, SeriesInfo info,
            AniDbSeriesData aniDbSeriesData)
        {
            return _tvDbClient.GetSeriesAsync(tvDbSeriesId)
                .MapAsync(tvDbSeries =>
                    _seriesMetadataFactory.CreateMetadata(aniDbSeriesData, tvDbSeries, info.MetadataLanguage));
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