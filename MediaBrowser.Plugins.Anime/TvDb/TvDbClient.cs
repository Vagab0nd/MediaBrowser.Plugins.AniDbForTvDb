using System.Collections.Generic;
using System.Threading.Tasks;
using Functional.Maybe;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.Anime.TvDb.Data;
using MediaBrowser.Plugins.Anime.TvDb.Requests;

namespace MediaBrowser.Plugins.Anime.TvDb
{
    internal class TvDbClient : ITvDbClient
    {
        private readonly TvDbToken _token;
        private readonly ITvDbConnection _tvDbConnection;

        public TvDbClient(ITvDbConnection tvDbConnection, ILogManager logManager)
        {
            _tvDbConnection = tvDbConnection;
            _token = new TvDbToken(_tvDbConnection, "E32490FAD276FF5E", logManager);
        }

        public async Task<Maybe<IEnumerable<TvDbEpisodeData>>> GetEpisodesAsync(int tvDbSeriesId)
        {
            var token = await _token.GetTokenAsync();

            var request = new GetEpisodesRequest(tvDbSeriesId, 1);

            var response = await _tvDbConnection.GetAsync(request, token);

            return response.Match(r => r.Data.Data.ToMaybe(),
                fr => Maybe<IEnumerable<TvDbEpisodeData>>.Nothing);
        }
    }
}