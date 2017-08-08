using System.Threading;
using System.Threading.Tasks;
using Functional.Maybe;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.Anime.AniDb.Data;
using MediaBrowser.Plugins.Anime.AniDb.Mapping;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    /// <summary>
    ///     Retrieves data from AniDb
    /// </summary>
    internal class AniDbClient : IAniDbClient
    {
        private readonly IAniDbDataCache _aniDbDataCache;
        private readonly IAnimeMappingListFactory _animeMappingListFactory;
        private readonly ILogger _log;
        private readonly ISeriesTitleCache _seriesTitleCache;

        public AniDbClient(IAniDbDataCache aniDbDataCache, IAnimeMappingListFactory animeMappingListFactory,
            ISeriesTitleCache seriesTitleCache, ILogManager logManager)
        {
            _aniDbDataCache = aniDbDataCache;
            _animeMappingListFactory = animeMappingListFactory;
            _seriesTitleCache = seriesTitleCache;
            _log = logManager.GetLogger(nameof(AniDbClient));
        }

        public Task<Maybe<AniDbSeries>> FindSeriesAsync(string title)
        {
            _log.Debug($"Finding AniDb series with title '{title}'");

            var matchedTitle = _seriesTitleCache.FindSeriesByTitle(title);

            var seriesTask = Task.FromResult(Maybe<AniDbSeries>.Nothing);

            matchedTitle.Match(
                t =>
                {
                    _log.Debug($"Found AniDb series Id '{t.AniDbId}' by title");

                    seriesTask = _aniDbDataCache.GetSeriesAsync(t.AniDbId, CancellationToken.None)
                        .ContinueWith(task => task.Result.ToMaybe());
                },
                () => _log.Debug("Failed to find AniDb series by title"));

            return seriesTask;
        }

        public Task<AniDbSeries> GetSeriesAsync(int aniDbSeriesId)
        {
            return _aniDbDataCache.GetSeriesAsync(aniDbSeriesId, CancellationToken.None);
        }

        public async Task<Maybe<AniDbSeries>> GetSeriesAsync(string aniDbSeriesIdString)
        {
            var aniDbSeries = !int.TryParse(aniDbSeriesIdString, out int aniDbSeriesId)
                ? Maybe<AniDbSeries>.Nothing
                : (await GetSeriesAsync(aniDbSeriesId)).ToMaybe();

            return aniDbSeries;
        }

        public async Task<AniDbMapper> GetMapperAsync()
        {
            var mappingList = await _animeMappingListFactory.CreateMappingListAsync(CancellationToken.None);

            return new AniDbMapper(mappingList);
        }
    }
}