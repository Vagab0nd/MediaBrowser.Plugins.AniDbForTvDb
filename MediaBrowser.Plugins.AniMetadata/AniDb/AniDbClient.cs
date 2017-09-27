using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping;
using MediaBrowser.Plugins.AniMetadata.AniDb.Seiyuu;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.AniDb.Titles;
using MediaBrowser.Plugins.AniMetadata.TvDb;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    /// <summary>
    ///     Retrieves data from AniDb
    /// </summary>
    internal class AniDbClient : IAniDbClient
    {
        private readonly IAniDbDataCache _aniDbDataCache;
        private readonly IAnimeMappingListFactory _animeMappingListFactory;
        private readonly ILogger _log;
        private readonly ILogManager _logManager;
        private readonly ISeriesTitleCache _seriesTitleCache;
        private readonly ITvDbClient _tvDbClient;

        public AniDbClient(IAniDbDataCache aniDbDataCache, IAnimeMappingListFactory animeMappingListFactory,
            ISeriesTitleCache seriesTitleCache, ITvDbClient tvDbClient, ILogManager logManager)
        {
            _aniDbDataCache = aniDbDataCache;
            _animeMappingListFactory = animeMappingListFactory;
            _seriesTitleCache = seriesTitleCache;
            _tvDbClient = tvDbClient;
            _logManager = logManager;
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

        public async Task<Option<IAniDbMapper>> GetMapperAsync()
        {
            var mappingList = await _animeMappingListFactory.CreateMappingListAsync(CancellationToken.None);

            return mappingList.Select(m => new AniDbMapper(m, _tvDbClient, _logManager) as IAniDbMapper);
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

        private Task<Option<AniDbSeriesData>> GetSeriesAsync(int aniDbSeriesId)
        {
            return _aniDbDataCache.GetSeriesAsync(aniDbSeriesId, CancellationToken.None);
        }
    }
}