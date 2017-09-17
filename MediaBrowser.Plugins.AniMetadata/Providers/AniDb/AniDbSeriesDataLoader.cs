using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using OneOf;

namespace MediaBrowser.Plugins.AniMetadata.Providers.AniDb
{
    internal class AniDbSeriesDataLoader : ISeriesDataLoader
    {
        private readonly IAniDbClient _aniDbClient;
        private readonly ILogger _log;
        private readonly ITvDbClient _tvDbClient;

        public AniDbSeriesDataLoader(ILogManager logManager, IAniDbClient aniDbClient, ITvDbClient tvDbClient)
        {
            _aniDbClient = aniDbClient;
            _tvDbClient = tvDbClient;
            _log = logManager.GetLogger(nameof(AniDbSeriesProvider));
        }

        public Task<OneOf<SeriesData, CombinedSeriesData, NoSeriesData>> GetSeriesDataAsync(SeriesInfo info)
        {
            return _aniDbClient.FindSeriesAsync(info.Name)
                .MatchAsync(aniDbSeriesData => _aniDbClient.GetMapperAsync()
                        .MatchAsync(mapper => mapper.GetMappedSeriesIdsFromAniDb(aniDbSeriesData.Id)
                                .MatchAsync(
                                    seriesIds =>
                                        GetSeriesDataAsync(seriesIds, aniDbSeriesData),
                                    () =>
                                    {
                                        _log.Debug("No series Id mappings found, using AniDb data only");
                                        return GetAniDbSeriesData(GetAniDbOnlySeriesIds(aniDbSeriesData.Id),
                                            aniDbSeriesData);
                                    }),
                            () =>
                            {
                                _log.Debug("Failed to load mapping list, using AniDb data only");
                                return GetAniDbSeriesData(GetAniDbOnlySeriesIds(aniDbSeriesData.Id), aniDbSeriesData);
                            }),
                    () =>
                    {
                        _log.Debug("Failed to find AniDb series by name");
                        return new NoSeriesData();
                    });
        }

        private Task<OneOf<SeriesData, CombinedSeriesData, NoSeriesData>> GetSeriesDataAsync(SeriesIds seriesIds,
            AniDbSeriesData aniDbSeriesData)
        {
            return seriesIds.TvDbSeriesId.MatchAsync(id => _tvDbClient.GetSeriesAsync(id)
                .MatchAsync(
                    s => (OneOf<SeriesData, CombinedSeriesData, NoSeriesData>)new CombinedSeriesData(seriesIds,
                        aniDbSeriesData, s),
                    () =>
                    {
                        _log.Debug($"Failed to load TvDb series with Id {id}, using AniDb data only");
                        return GetAniDbSeriesData(seriesIds, aniDbSeriesData);
                    }), () =>
            {
                _log.Debug("No TvDb series Id mapped, using AniDb data only");
                return GetAniDbSeriesData(seriesIds, aniDbSeriesData);
            });
        }

        private SeriesIds GetAniDbOnlySeriesIds(int anidbId)
        {
            return new SeriesIds(anidbId, Option<int>.None, Option<int>.None, Option<int>.None);
        }

        private OneOf<SeriesData, CombinedSeriesData, NoSeriesData> GetAniDbSeriesData(SeriesIds seriesIds,
            AniDbSeriesData aniDbSeriesData)
        {
            return new SeriesData(seriesIds, aniDbSeriesData);
        }
    }
}