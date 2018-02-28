using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Mapping;

namespace MediaBrowser.Plugins.AniMetadata.Providers.AniDb
{
    internal class AniDbSeriesDataLoader : ISeriesDataLoader
    {
        private readonly IAniDbClient _aniDbClient;
        private readonly IDataMapperFactory _dataMapperFactory;
        private readonly ILogger _log;

        public AniDbSeriesDataLoader(ILogManager logManager, IAniDbClient aniDbClient, IDataMapperFactory dataMapperFactory)
        {
            _aniDbClient = aniDbClient;
            _dataMapperFactory = dataMapperFactory;
            _log = logManager.GetLogger(nameof(AniDbSeriesProvider));
        }

        public Task<SeriesData> GetSeriesDataAsync(string seriesName)
        {
            return _aniDbClient.FindSeriesAsync(seriesName)
                .MatchAsync(GetSeriesDataAsync,
                    () =>
                    {
                        _log.Debug("Failed to find AniDb series by name");
                        return new NoSeriesData();
                    });
        }

        public Task<SeriesData> GetSeriesDataAsync(int aniDbSeriesId)
        {
            return _aniDbClient.GetSeriesAsync(aniDbSeriesId)
                .MatchAsync(GetSeriesDataAsync,
                    () =>
                    {
                        _log.Debug($"Failed to load AniDb series with Id {aniDbSeriesId}");
                        return new NoSeriesData();
                    });
        }

        private Task<SeriesData> GetSeriesDataAsync(AniDbSeriesData aniDbSeriesData)
        {
            return _dataMapperFactory.GetDataMapperAsync()
                .Match(mapper => mapper.MapSeriesDataAsync(aniDbSeriesData),
                    () => new AniDbOnlySeriesData(
                        new SeriesIds(aniDbSeriesData.Id, Option<int>.None, Option<int>.None, Option<int>.None),
                        aniDbSeriesData));
        }
    }
}