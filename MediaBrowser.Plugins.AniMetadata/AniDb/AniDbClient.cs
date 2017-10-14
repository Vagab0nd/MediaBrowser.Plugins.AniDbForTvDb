using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.AniDb.Seiyuu;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.AniDb.Titles;
using MediaBrowser.Plugins.AniMetadata.Mapping;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    /// <summary>
    ///     Retrieves data from AniDb
    /// </summary>
    internal class AniDbClient : IAniDbClient
    {
        private readonly IAniDbDataCache _aniDbDataCache;
        private readonly IAnimeMappingListFactory _animeMappingListFactory;
        private readonly IDataMapperFactory _dataMapperFactory;
        private readonly ILogger _log;
        private readonly ISeriesTitleCache _seriesTitleCache;

        public AniDbClient(IAniDbDataCache aniDbDataCache, IAnimeMappingListFactory animeMappingListFactory,
            ISeriesTitleCache seriesTitleCache, IDataMapperFactory dataMapperFactory, ILogManager logManager)
        {
            _aniDbDataCache = aniDbDataCache;
            _animeMappingListFactory = animeMappingListFactory;
            _seriesTitleCache = seriesTitleCache;
            _dataMapperFactory = dataMapperFactory;
            _log = logManager.GetLogger(nameof(AniDbClient));
        }

        public Task<Option<AniDbSeriesData>> FindSeriesAsync(string title)
        {
            _log.Debug($"Finding AniDb series with title '{title}'");

            var matchedTitle = _seriesTitleCache.FindSeriesByTitle(title);

            var seriesTask = Task.FromResult(Option<AniDbSeriesData>.None);

            matchedTitle.Match(
                t =>
                {
                    _log.Debug($"Found AniDb series Id '{t.AniDbId}' by title");

                    seriesTask = _aniDbDataCache.GetSeriesAsync(t.AniDbId, CancellationToken.None);
                },
                () => _log.Debug("Failed to find AniDb series by title"));

            return seriesTask;
        }

        public async Task<Option<AniDbSeriesData>> GetSeriesAsync(string aniDbSeriesIdString)
        {
            var aniDbSeries = !int.TryParse(aniDbSeriesIdString, out var aniDbSeriesId)
                ? Option<AniDbSeriesData>.None
                : await GetSeriesAsync(aniDbSeriesId);

            return aniDbSeries;
        }

        public Task<Option<AniDbSeriesData>> GetSeriesAsync(int aniDbSeriesId)
        {
            return _aniDbDataCache.GetSeriesAsync(aniDbSeriesId, CancellationToken.None);
        }

        public async Task<Option<IDataMapper>> GetMapperAsync()
        {
            var mappingList = await _animeMappingListFactory.CreateMappingListAsync(CancellationToken.None);

            return mappingList.Select(m => _dataMapperFactory.GetDataMapper(m));
        }

        public IEnumerable<SeiyuuData> FindSeiyuu(string name)
        {
            name = name.ToUpperInvariant();

            return _aniDbDataCache.GetSeiyuu().Where(s => s.Name.ToUpperInvariant().Contains(name));
        }

        public Option<SeiyuuData> GetSeiyuu(int seiyuuId)
        {
            return _aniDbDataCache.GetSeiyuu().Find(s => s.Id == seiyuuId);
        }
    }
}