using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.AniDb.Seiyuu;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.AniDb.Titles;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    /// <summary>
    ///     Retrieves data from AniDb
    /// </summary>
    internal class AniDbClient : IAniDbClient
    {
        private readonly IAniDbDataCache aniDbDataCache;
        private readonly ILogger log;
        private readonly ISeriesTitleCache seriesTitleCache;

        public AniDbClient(IAniDbDataCache aniDbDataCache,
            ISeriesTitleCache seriesTitleCache, ILogManager logManager)
        {
            this.aniDbDataCache = aniDbDataCache;
            this.seriesTitleCache = seriesTitleCache;
            this.log = logManager.GetLogger(nameof(AniDbClient));
        }

        public Task<Option<AniDbSeriesData>> FindSeriesAsync(string title)
        {
            this.log.Debug($"Finding AniDb series with title '{title}'");

            var matchedTitle = this.seriesTitleCache.FindSeriesByTitle(title);

            var seriesTask = Task.FromResult(Option<AniDbSeriesData>.None);

            matchedTitle.Match(
                t =>
                {
                    this.log.Debug($"Found AniDb series Id '{t.AniDbId}' by title");

                    seriesTask = this.aniDbDataCache.GetSeriesAsync(t.AniDbId, CancellationToken.None);
                },
                () => this.log.Debug("Failed to find AniDb series by title"));

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
            return this.aniDbDataCache.GetSeriesAsync(aniDbSeriesId, CancellationToken.None);
        }

        public IEnumerable<SeiyuuData> FindSeiyuu(string name)
        {
            name = name.ToUpperInvariant();

            return this.aniDbDataCache.GetSeiyuu().Where(s => s.Name.ToUpperInvariant().Contains(name));
        }

        public Option<SeiyuuData> GetSeiyuu(int seiyuuId)
        {
            return this.aniDbDataCache.GetSeiyuu().Find(s => s.Id == seiyuuId);
        }
    }
}