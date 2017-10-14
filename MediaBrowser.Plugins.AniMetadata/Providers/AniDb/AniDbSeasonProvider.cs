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
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Mapping;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using OneOf;
using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata.Providers.AniDb
{
    internal class AniDbSeasonProvider : IRemoteMetadataProvider<Season, SeasonInfo>
    {
        private readonly IAniDbClient _aniDbClient;
        private readonly ISeasonMetadataFactory _seasonMetadataFactory;
        private readonly ITvDbClient _tvDbClient;
        private readonly ILogger _log;

        public AniDbSeasonProvider(IAniDbClient aniDbClient, ITvDbClient tvDbClient,
            ISeasonMetadataFactory seasonMetadataFactory, ILogManager logManager)
        {
            _aniDbClient = aniDbClient;
            _tvDbClient = tvDbClient;
            _seasonMetadataFactory = seasonMetadataFactory;
            _log = logManager.GetLogger(nameof(AniDbSeasonProvider));
        }

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeasonInfo searchInfo,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<MetadataResult<Season>> GetMetadata(SeasonInfo info, CancellationToken cancellationToken)
        {
            _log.Debug($"Finding data for season {info.IndexNumber.GetValueOrDefault(1)}");

            var aniDbSeriesId = parseInt(info.SeriesProviderIds.GetOrDefault(ProviderNames.AniDb));

            var result = aniDbSeriesId.MatchAsync(async id =>
            {
                var tvDbSeriesId = ((IDictionary<string, string>)info.SeriesProviderIds)
                    .TryGetValue(MetadataProviders.Tvdb.ToString())
                    .Bind(parseInt);

                var seriesIds = new SeriesIds(id, tvDbSeriesId, Option<int>.None, Option<int>.None);

                var aniDbSeriesData = await _aniDbClient.GetSeriesAsync(id);

                var seriesData = await GetSeriesDataAsync(seriesIds, aniDbSeriesData);

                return seriesData.Match(
                    singleSeriesData =>
                    {
                        _log.Debug("Found AniDb data");
                        return _seasonMetadataFactory.CreateMetadata(singleSeriesData.AniDbSeriesData,
                            info.IndexNumber.GetValueOrDefault(1), info.MetadataLanguage);
                    },
                    combinedSeriesData =>
                    {
                        _log.Debug("Found AniDb and TvDb data");
                        return _seasonMetadataFactory.CreateMetadata(combinedSeriesData.AniDbSeriesData,
                            combinedSeriesData.TvDbSeriesData, info.IndexNumber.GetValueOrDefault(1), info.MetadataLanguage);
                    },
                    noData => _seasonMetadataFactory.NullResult);
            }, () =>
            {
                _log.Debug("Failed to find AniDb series Id");
                return _seasonMetadataFactory.NullResult;
            });

            return result;
        }

        public string Name => ProviderNames.AniDb;

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private Task<OneOf<AniDbOnlySeriesData, CombinedSeriesData, NoSeriesData>> GetSeriesDataAsync(SeriesIds seriesIds,
            Option<AniDbSeriesData> aniDbSeriesData)
        {
            return aniDbSeriesData.MatchAsync(aniDbData => seriesIds.TvDbSeriesId.MatchAsync(id =>
                        _tvDbClient.GetSeriesAsync(id)
                            .MatchAsync(
                                s => (OneOf<AniDbOnlySeriesData, CombinedSeriesData, NoSeriesData>)new CombinedSeriesData(
                                    seriesIds,
                                    aniDbData, s),
                                () => new AniDbOnlySeriesData(seriesIds, aniDbData)),
                    () => new AniDbOnlySeriesData(seriesIds, aniDbData)),
                () => new NoSeriesData());
        }
    }
}