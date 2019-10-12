using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.Files;
using MediaBrowser.Plugins.AniMetadata.JsonApi;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;
using MediaBrowser.Plugins.AniMetadata.TvDb.Requests;

namespace MediaBrowser.Plugins.AniMetadata.TvDb
{
    using Infrastructure;

    internal class TvDbClient : ITvDbClient
    {
        private readonly IApplicationPaths applicationPaths;
        private readonly IFileCache fileCache;
        private readonly ICustomJsonSerialiser jsonSerialiser;
        private readonly ILogger log;
        private readonly TvDbToken token;
        private readonly IJsonConnection jsonConnection;

        public TvDbClient(IJsonConnection jsonConnection, IFileCache fileCache, IApplicationPaths applicationPaths,
            ILogManager logManager, ICustomJsonSerialiser jsonSerialiser, PluginConfiguration configuration)
        {
            this.log = logManager.GetLogger(nameof(TvDbClient));
            this.jsonConnection = jsonConnection;
            this.fileCache = fileCache;
            this.applicationPaths = applicationPaths;
            this.jsonSerialiser = jsonSerialiser;
            this.token = new TvDbToken(this.jsonConnection, configuration.TvDbApiKey, logManager);
        }

        public async Task<Option<TvDbSeriesData>> GetSeriesAsync(int tvDbSeriesId)
        {
            var localSeriesData = GetLocalTvDbSeriesData(tvDbSeriesId);

            var episodes = await localSeriesData.MatchAsync(e => e,
                async () =>
                {
                    var seriesData = await RequestSeriesAsync(tvDbSeriesId);

                    seriesData.Iter(SaveTvDbSeries);

                    return seriesData;
                });

            return episodes;
        }

        public Task<Option<TvDbSeriesData>> FindSeriesAsync(string seriesName)
        {
            return this.token.GetTokenAsync()
                .Bind(t => this.jsonConnection.GetAsync(new FindSeriesRequest(seriesName), t)
                    .Bind(response =>
                    {
                        return response.Match(
                            r => r.Data.MatchingSeries.Aggregate(Task.FromResult(Option<TvDbSeriesData>.None),
                                (existing, current) => existing.Bind(e => e.Match(s =>
                                    {
                                        this.log.Debug(
                                            $"More than one matching series found for series name '{seriesName}'");
                                        return Task.FromResult(Option<TvDbSeriesData>.None);
                                    },
                                    () => GetSeriesAsync(current.Id)))),
                            fr => Task.FromResult(Option<TvDbSeriesData>.None));
                    }));
        }

        public Task<Option<List<TvDbEpisodeData>>> GetEpisodesAsync(int tvDbSeriesId)
        {
            var localEpisodes = GetLocalTvDbEpisodeData(tvDbSeriesId).Map(e => e.ToList());

            var episodes = localEpisodes.Match(e => Task.FromResult((Option<List<TvDbEpisodeData>>)e),
                () => RequestEpisodesAsync(tvDbSeriesId));

            return episodes;
        }

        public async Task<Option<TvDbEpisodeData>> GetEpisodeAsync(int tvDbSeriesId, int seasonIndex,
            int episodeIndex)
        {
            var episodes = await GetEpisodesAsync(tvDbSeriesId);

            return episodes.Match(ec =>
                    ec.Find(e => e.AiredSeason == seasonIndex && e.AiredEpisodeNumber == episodeIndex),
                () => Option<TvDbEpisodeData>.None);
        }

        public async Task<Option<TvDbEpisodeData>> GetEpisodeAsync(int tvDbSeriesId, int absoluteEpisodeIndex)
        {
            var episodes = await GetEpisodesAsync(tvDbSeriesId);

            return episodes.Match(ec => ((IEnumerable<TvDbEpisodeData>)ec).Find(e =>
                    e.AbsoluteNumber.Match(index => index == absoluteEpisodeIndex, () => false)),
                () => Option<TvDbEpisodeData>.None);
        }

        private async Task<Option<TvDbSeriesData>> RequestSeriesAsync(int tvDbSeriesId)
        {
            var token = await this.token.GetTokenAsync();

            var request = new GetSeriesRequest(tvDbSeriesId);

            var response = await this.jsonConnection.GetAsync(request, token);

            return response.Match(
                r => r.Data.Data,
                fr => Option<TvDbSeriesData>.None);
        }

        private Option<IEnumerable<TvDbEpisodeData>> GetLocalTvDbEpisodeData(int tvDbSeriesId)
        {
            var fileSpec = new TvDbSeriesEpisodesFileSpec(this.jsonSerialiser, this.applicationPaths.CachePath, tvDbSeriesId);

            return this.fileCache.GetFileContent(fileSpec).Select(c => c.Episodes);
        }

        private Option<TvDbSeriesData> GetLocalTvDbSeriesData(int tvDbSeriesId)
        {
            var fileSpec = new TvDbSeriesFileSpec(this.jsonSerialiser, this.applicationPaths.CachePath, tvDbSeriesId);

            return this.fileCache.GetFileContent(fileSpec);
        }

        private async Task<Option<List<TvDbEpisodeData>>> RequestEpisodesAsync(int tvDbSeriesId)
        {
            var token = await this.token.GetTokenAsync();

            var request = new GetEpisodesRequest(tvDbSeriesId, 1);

            var response = await this.jsonConnection.GetAsync(request, token);

            return await response.Match(async r =>
                {
                    var episodes = r.Data.Data.ToList();

                    if (r.Data.Links.Last > 1)
                    {
                        episodes = episodes.Concat(await RequestEpisodePagesAsync(tvDbSeriesId, 2, r.Data.Links.Last,
                                token))
                            .ToList();
                    }

                    var episodeDetails = (await episodes.Map(e => e.Id).Map(RequestEpisodeDetailAsync)).Somes().ToList();

                    SaveTvDbEpisodes(tvDbSeriesId, episodeDetails);

                    return (Option<List<TvDbEpisodeData>>)episodeDetails.ToList();
                },
                fr => Task.FromResult(Option<List<TvDbEpisodeData>>.None));
        }

        private async Task<Option<TvDbEpisodeData>> RequestEpisodeDetailAsync(int episodeId)
        {
            var token = await this.token.GetTokenAsync();

            var request = new GetEpisodeDetailsRequest(episodeId);

            var response = await this.jsonConnection.GetAsync(request, token);

            return response.Match(r => r.Data.Data,
                fr => null);
        }

        private void SaveTvDbEpisodes(int tvDbSeriesId, IEnumerable<TvDbEpisodeData> episodes)
        {
            var fileSpec = new TvDbSeriesEpisodesFileSpec(this.jsonSerialiser, this.applicationPaths.CachePath, tvDbSeriesId);

            this.fileCache.SaveFile(fileSpec, new TvDbEpisodeCollection(episodes));
        }

        private void SaveTvDbSeries(TvDbSeriesData tvDbSeries)
        {
            var fileSpec = new TvDbSeriesFileSpec(this.jsonSerialiser, this.applicationPaths.CachePath, tvDbSeries.Id);

            this.fileCache.SaveFile(fileSpec, tvDbSeries);
        }

        private async Task<IEnumerable<TvDbEpisodeSummaryData>> RequestEpisodePagesAsync(int tvDbSeriesId,
            int startPageIndex, int endPageIndex, Option<string> token)
        {
            var episodeData = new List<TvDbEpisodeSummaryData>();

            for (var i = startPageIndex; i <= endPageIndex; i++)
            {
                var pageEpisodes = await RequestEpisodesPageAsync(tvDbSeriesId, i, token);

                pageEpisodes.Iter(e => episodeData.AddRange(e));
            }

            return episodeData;
        }

        private async Task<Option<List<TvDbEpisodeSummaryData>>> RequestEpisodesPageAsync(int tvDbSeriesId,
            int pageIndex, Option<string> token)
        {
            var request = new GetEpisodesRequest(tvDbSeriesId, pageIndex);

            var response = await this.jsonConnection.GetAsync(request, token);

            return response.Match(r => r.Data.Data.ToList(),
                fr => Option<List<TvDbEpisodeSummaryData>>.None);
        }
    }
}