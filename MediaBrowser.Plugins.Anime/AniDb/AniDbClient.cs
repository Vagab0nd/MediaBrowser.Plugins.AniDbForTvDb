using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Functional.Maybe;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.Anime.AniDb.Mapping;
using MediaBrowser.Plugins.Anime.AniDb.Seiyuu;
using MediaBrowser.Plugins.Anime.AniDb.Series;
using MediaBrowser.Plugins.Anime.AniDb.Series.Data;
using MediaBrowser.Plugins.Anime.AniDb.Titles;

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

        public Task<Maybe<AniDbSeriesData>> FindSeriesAsync(string title)
        {
            _log.Debug($"Finding AniDb series with title '{title}'");

            var matchedTitle = _seriesTitleCache.FindSeriesByTitle(title);

            var seriesTask = Task.FromResult(Maybe<AniDbSeriesData>.Nothing);

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

        public Task<AniDbSeriesData> GetSeriesAsync(int aniDbSeriesId)
        {
            return _aniDbDataCache.GetSeriesAsync(aniDbSeriesId, CancellationToken.None);
        }

        public async Task<Maybe<AniDbSeriesData>> GetSeriesAsync(string aniDbSeriesIdString)
        {
            var aniDbSeries = !int.TryParse(aniDbSeriesIdString, out int aniDbSeriesId)
                ? Maybe<AniDbSeriesData>.Nothing
                : (await GetSeriesAsync(aniDbSeriesId)).ToMaybe();

            return aniDbSeries;
        }

        public async Task<AniDbMapper> GetMapperAsync()
        {
            var mappingList = await _animeMappingListFactory.CreateMappingListAsync(CancellationToken.None);

            return new AniDbMapper(mappingList);
        }

        public IEnumerable<SeiyuuData> FindSeiyuu(string name)
        {
            name = name.ToUpperInvariant();

            return _aniDbDataCache.GetSeiyuu().Where(s => s.Name.ToUpperInvariant().Contains(name));
        }

        public Maybe<SeiyuuData> GetSeiyuu(int seiyuuId)
        {
            return _aniDbDataCache.GetSeiyuu().FirstMaybe(s => s.Id == seiyuuId);
        }
    }
}