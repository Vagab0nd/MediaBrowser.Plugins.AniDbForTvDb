using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.Files;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;
using MediaBrowser.Plugins.AniMetadata.TvDb.Requests;

namespace MediaBrowser.Plugins.AniMetadata.TvDb
{
    internal class TvDbClient : ITvDbClient
    {
        private readonly IApplicationPaths _applicationPaths;
        private readonly IFileCache _fileCache;
        private readonly ICustomJsonSerialiser _jsonSerialiser;
        private readonly ILogger _log;
        private readonly TvDbToken _token;
        private readonly ITvDbConnection _tvDbConnection;

        public TvDbClient(ITvDbConnection tvDbConnection, IFileCache fileCache, IApplicationPaths applicationPaths,
            ILogManager logManager, ICustomJsonSerialiser jsonSerialiser, PluginConfiguration configuration)
        {
            _log = logManager.GetLogger(nameof(TvDbClient));
            _tvDbConnection = tvDbConnection;
            _fileCache = fileCache;
            _applicationPaths = applicationPaths;
            _jsonSerialiser = jsonSerialiser;
            _token = new TvDbToken(_tvDbConnection, configuration.TvDbApiKey, logManager);
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
            return _token.GetTokenAsync()
                .Bind(t => _tvDbConnection.GetAsync(new FindSeriesRequest(seriesName), t)
                    .Bind(response =>
                    {
                        return response.Match(
                            r => r.Data.MatchingSeries.Aggregate(Task.FromResult(Option<TvDbSeriesData>.None),
                                (existing, current) => existing.Bind(e => e.Match(s =>
                                    {
                                        _log.Debug(
                                            $"More than one matching series found for series name '{seriesName}'");
                                        return Task.FromResult(Option<TvDbSeriesData>.None);
                                    },
                                    () => GetSeriesAsync(current.Id)))),
                            fr => Task.FromResult(Option<TvDbSeriesData>.None));
                    }));
        }

        public Task<Option<List<TvDbEpisodeDetailData>>> GetEpisodesAsync(int tvDbSeriesId)
        {
            var localEpisodes = GetLocalTvDbEpisodeData(tvDbSeriesId).Map(e => e.ToList());

            var episodes = localEpisodes.Match(e => Task.FromResult((Option<List<TvDbEpisodeDetailData>>)e),
                () => RequestEpisodesAsync(tvDbSeriesId));

            return episodes;
        }

        private async Task<Option<TvDbSeriesData>> RequestSeriesAsync(int tvDbSeriesId)
        {
            var token = await _token.GetTokenAsync();

            var request = new GetSeriesRequest(tvDbSeriesId);

            var response = await _tvDbConnection.GetAsync(request, token);

            return response.Match(
                r => r.Data.Data,
                fr => Option<TvDbSeriesData>.None);
        }

        private Option<IEnumerable<TvDbEpisodeDetailData>> GetLocalTvDbEpisodeData(int tvDbSeriesId)
        {
            var fileSpec = new TvDbSeriesEpisodesFileSpec(_jsonSerialiser, _applicationPaths.CachePath, tvDbSeriesId);

            return _fileCache.GetFileContent(fileSpec).Select(c => c.Episodes);
        }

        private Option<TvDbSeriesData> GetLocalTvDbSeriesData(int tvDbSeriesId)
        {
            var fileSpec = new TvDbSeriesFileSpec(_jsonSerialiser, _applicationPaths.CachePath, tvDbSeriesId);

            return _fileCache.GetFileContent(fileSpec);
        }

        private async Task<Option<List<TvDbEpisodeDetailData>>> RequestEpisodesAsync(int tvDbSeriesId)
        {
            var token = await _token.GetTokenAsync();

            var request = new GetEpisodesRequest(tvDbSeriesId, 1);

            var response = await _tvDbConnection.GetAsync(request, token);

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

                    return (Option<List<TvDbEpisodeDetailData>>)episodeDetails.ToList();
                },
                fr => Task.FromResult(Option<List<TvDbEpisodeDetailData>>.None));
        }

        private async Task<Option<TvDbEpisodeDetailData>> RequestEpisodeDetailAsync(int episodeId)
        {
            var token = await _token.GetTokenAsync();

            var request = new GetEpisodeDetailsRequest(episodeId);

            var response = await _tvDbConnection.GetAsync(request, token);

            return response.Match(r => r.Data.Data,
                fr => null);
        }

        private void SaveTvDbEpisodes(int tvDbSeriesId, IEnumerable<TvDbEpisodeDetailData> episodes)
        {
            var fileSpec = new TvDbSeriesEpisodesFileSpec(_jsonSerialiser, _applicationPaths.CachePath, tvDbSeriesId);

            _fileCache.SaveFile(fileSpec, new TvDbEpisodeCollection(episodes));
        }

        private void SaveTvDbSeries(TvDbSeriesData tvDbSeries)
        {
            var fileSpec = new TvDbSeriesFileSpec(_jsonSerialiser, _applicationPaths.CachePath, tvDbSeries.Id);

            _fileCache.SaveFile(fileSpec, tvDbSeries);
        }

        private async Task<IEnumerable<TvDbEpisodeData>> RequestEpisodePagesAsync(int tvDbSeriesId,
            int startPageIndex, int endPageIndex, Option<string> token)
        {
            var episodeData = new List<TvDbEpisodeData>();

            for (var i = startPageIndex; i <= endPageIndex; i++)
            {
                var pageEpisodes = await RequestEpisodesPageAsync(tvDbSeriesId, i, token);

                pageEpisodes.Iter(e => episodeData.AddRange(e));
            }

            return episodeData;
        }

        private async Task<Option<List<TvDbEpisodeData>>> RequestEpisodesPageAsync(int tvDbSeriesId,
            int pageIndex, Option<string> token)
        {
            var request = new GetEpisodesRequest(tvDbSeriesId, pageIndex);

            var response = await _tvDbConnection.GetAsync(request, token);

            return response.Match(r => r.Data.Data.ToList(),
                fr => Option<List<TvDbEpisodeData>>.None);
        }
    }
}