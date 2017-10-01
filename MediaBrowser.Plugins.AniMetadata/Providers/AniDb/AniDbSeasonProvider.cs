using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
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

        public AniDbSeasonProvider(IAniDbClient aniDbClient, ITvDbClient tvDbClient,
            ISeasonMetadataFactory seasonMetadataFactory)
        {
            _aniDbClient = aniDbClient;
            _tvDbClient = tvDbClient;
            _seasonMetadataFactory = seasonMetadataFactory;
        }

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeasonInfo searchInfo,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<MetadataResult<Season>> GetMetadata(SeasonInfo info, CancellationToken cancellationToken)
        {
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
                        _seasonMetadataFactory.CreateMetadata(singleSeriesData.AniDbSeriesData,
                            info.IndexNumber.GetValueOrDefault(1)),
                    combinedSeriesData =>
                        _seasonMetadataFactory.CreateMetadata(combinedSeriesData.AniDbSeriesData,
                            combinedSeriesData.TvDbSeriesData, info.IndexNumber.GetValueOrDefault(1)),
                    noData => _seasonMetadataFactory.NullResult);
            }, () => _seasonMetadataFactory.NullResult);

            return result;
        }

        public string Name => ProviderNames.AniDb;

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private Task<OneOf<SeriesData, CombinedSeriesData, NoSeriesData>> GetSeriesDataAsync(SeriesIds seriesIds,
            Option<AniDbSeriesData> aniDbSeriesData)
        {
            return aniDbSeriesData.MatchAsync(aniDbData => seriesIds.TvDbSeriesId.MatchAsync(id =>
                        _tvDbClient.GetSeriesAsync(id)
                            .MatchAsync(
                                s => (OneOf<SeriesData, CombinedSeriesData, NoSeriesData>)new CombinedSeriesData(
                                    seriesIds,
                                    aniDbData, s),
                                () => new SeriesData(seriesIds, aniDbData)),
                    () => new SeriesData(seriesIds, aniDbData)),
                () => new NoSeriesData());
        }
    }
}