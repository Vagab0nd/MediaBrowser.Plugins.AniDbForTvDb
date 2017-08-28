using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Functional.Maybe;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.Anime.Files;
using MediaBrowser.Plugins.Anime.TvDb.Data;
using MediaBrowser.Plugins.Anime.TvDb.Requests;

namespace MediaBrowser.Plugins.Anime.TvDb
{
    internal class TvDbClient : ITvDbClient
    {
        private readonly IApplicationPaths _applicationPaths;
        private readonly IFileCache _fileCache;
        private readonly TvDbToken _token;
        private readonly ITvDbConnection _tvDbConnection;

        public TvDbClient(ITvDbConnection tvDbConnection, IFileCache fileCache, IApplicationPaths applicationPaths,
            ILogManager logManager)
        {
            _tvDbConnection = tvDbConnection;
            _fileCache = fileCache;
            _applicationPaths = applicationPaths;
            _token = new TvDbToken(_tvDbConnection, "E32490FAD276FF5E", logManager);
        }

        public async Task<Maybe<IEnumerable<TvDbEpisodeData>>> GetEpisodesAsync(int tvDbSeriesId)
        {
            var localEpisodes = GetLocalTvDbSeriesData(tvDbSeriesId);

            var episodes = await localEpisodes.SelectOrElse(e => Task.FromResult(e.ToMaybe()),
                () => RequestEpisodesAsync(tvDbSeriesId));

            return episodes;
        }

        private Maybe<IEnumerable<TvDbEpisodeData>> GetLocalTvDbSeriesData(int tvDbSeriesId)
        {
            var fileSpec = new TvDbSeriesEpisodesFileSpec(_applicationPaths.CachePath, tvDbSeriesId);

            return _fileCache.GetFileContent(fileSpec).Select(c => c.Episodes);
        }

        private async Task<Maybe<IEnumerable<TvDbEpisodeData>>> RequestEpisodesAsync(int tvDbSeriesId)
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
                            token)).ToList();
                    }

                    SaveTvDbEpisodes(tvDbSeriesId, episodes);

                    return episodes.AsEnumerable().ToMaybe();
                },
                fr => Task.FromResult(Maybe<IEnumerable<TvDbEpisodeData>>.Nothing));
        }

        private void SaveTvDbEpisodes(int tvDbSeriesId, IEnumerable<TvDbEpisodeData> episodes)
        {
            var fileSpec = new TvDbSeriesEpisodesFileSpec(_applicationPaths.CachePath, tvDbSeriesId);

            _fileCache.SaveFile(fileSpec, new TvDbSeriesData(episodes));
        }

        private async Task<IEnumerable<TvDbEpisodeData>> RequestEpisodePagesAsync(int tvDbSeriesId,
            int startPageIndex, int endPageIndex, Maybe<string> token)
        {
            var episodeData = new List<TvDbEpisodeData>();

            for (int i = startPageIndex; i <= endPageIndex; i++)
            {
                var pageEpisodes = await RequestEpisodesPageAsync(tvDbSeriesId, i, token);

                pageEpisodes.Do(e => episodeData.AddRange(e));
            }

            return episodeData;
        }

        private async Task<Maybe<IEnumerable<TvDbEpisodeData>>> RequestEpisodesPageAsync(int tvDbSeriesId,
            int pageIndex, Maybe<string> token)
        {
            var request = new GetEpisodesRequest(tvDbSeriesId, pageIndex);

            var response = await _tvDbConnection.GetAsync(request, token);

            return response.Match(r => r.Data.Data.ToMaybe(),
                fr => Maybe<IEnumerable<TvDbEpisodeData>>.Nothing);
        }
    }
}