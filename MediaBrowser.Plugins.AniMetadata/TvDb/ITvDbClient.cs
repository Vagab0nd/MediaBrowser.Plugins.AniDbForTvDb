using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.TvDb
{
    public interface ITvDbClient
    {
        Task<Option<TvDbSeriesData>> GetSeriesAsync(int tvDbSeriesId);

        Task<Option<List<TvDbEpisodeData>>> GetEpisodesAsync(int tvDbSeriesId);
    }
}