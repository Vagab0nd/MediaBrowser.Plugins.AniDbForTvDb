using System.Collections.Generic;
using System.Threading.Tasks;
using Functional.Maybe;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.TvDb
{
    public interface ITvDbClient
    {
        Task<Maybe<IEnumerable<TvDbEpisodeData>>> GetEpisodesAsync(int tvDbSeriesId);
    }
}