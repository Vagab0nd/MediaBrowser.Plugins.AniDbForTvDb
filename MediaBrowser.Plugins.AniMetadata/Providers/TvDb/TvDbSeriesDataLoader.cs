using System.Threading.Tasks;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping;
using MediaBrowser.Plugins.AniMetadata.Providers.AniDb;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.Providers.TvDb
{
    internal class TvDbSeriesDataLoader : ISeriesDataLoader
    {
        private readonly IAniDbClient _aniDbClient;
        private readonly ILogger _log;
        private readonly ITvDbClient _tvDbClient;

        public TvDbSeriesDataLoader(ILogManager logManager, IAniDbClient aniDbClient, ITvDbClient tvDbClient)
        {
            _aniDbClient = aniDbClient;
            _tvDbClient = tvDbClient;
            _log = logManager.GetLogger(nameof(AniDbSeriesProvider));
        }

        public Task<SeriesData> GetSeriesDataAsync(string seriesName)
        {
            return _tvDbClient.FindSeriesAsync(seriesName)
                .MatchAsync(tvDbSeriesData => _aniDbClient.GetMapperAsync()
                        .MatchAsync(mapper => mapper.GetMappedSeriesIdsFromTvDb(tvDbSeriesData.Id)
                                .MatchAsync(
                                    seriesIds =>
                                        GetSeriesDataAsync(seriesIds, tvDbSeriesData),
                                    () =>
                                    {
                                        _log.Debug("No series Id mappings found");
                                        return new NoSeriesData();
                                    }),
                            () =>
                            {
                                _log.Debug("Failed to load mapping list");
                                return new NoSeriesData();
                            }),
                    () =>
                    {
                        _log.Debug("Failed to find TvDb series by name");
                        return new NoSeriesData();
                    });
        }

        private Task<SeriesData> GetSeriesDataAsync(SeriesIds seriesIds,
            TvDbSeriesData tvDbSeriesData)
        {
            return _aniDbClient.GetSeriesAsync(seriesIds.AniDbSeriesId.ToString())
                .MatchAsync(aniDbSeriesData =>
                        (SeriesData)new CombinedSeriesData(seriesIds, aniDbSeriesData, tvDbSeriesData),
                    () =>
                    {
                        _log.Debug($"Failed to load AniDb series with Id {seriesIds.AniDbSeriesId}");
                        return new NoSeriesData();
                    });
        }
    }
}