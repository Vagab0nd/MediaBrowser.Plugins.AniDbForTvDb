using System.Collections.Generic;
using System.Threading.Tasks;
using Functional.Maybe;
using MediaBrowser.Plugins.Anime.TvDb.Data;

namespace MediaBrowser.Plugins.Anime.TvDb
{
    public interface ITvDbClient
    {
        Task<Maybe<IEnumerable<TvDbEpisodeData>>> GetEpisodesAsync(int tvDbSeriesId);
    }
}