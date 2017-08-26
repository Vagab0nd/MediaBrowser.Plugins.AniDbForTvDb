using System.Collections.Generic;
using System.Threading.Tasks;
using Functional.Maybe;
using MediaBrowser.Plugins.Anime.TvDb.Data;
using MediaBrowser.Plugins.Anime.TvDb.Requests;

namespace MediaBrowser.Plugins.Anime.TvDb
{
    internal class TvDbSession
    {
        private readonly TvDbToken _token;
        private readonly ITvDbConnection _tvDbConnection;

        public TvDbSession(ITvDbConnection tvDbConnection)
        {
            _tvDbConnection = tvDbConnection;
            _token = new TvDbToken(_tvDbConnection, "E32490FAD276FF5E");
        }

        public async Task<Maybe<IEnumerable<TvDbEpisodeData>>> GetEpisodes(int tvDbSeriesId)
        {
            await _token.GetTokenAsync();

            var request = new GetEpisodesRequest(tvDbSeriesId, 1);

            var response = await _tvDbConnection.GetAsync(request);

            return response.Match(r => r.Data.Data.ToMaybe(),
                fr => Maybe<IEnumerable<TvDbEpisodeData>>.Nothing);
        }
    }
}