using System.Collections.Generic;

namespace MediaBrowser.Plugins.Anime.TvDb.Data
{
    internal class TvDbSeriesData
    {
        public TvDbSeriesData(IEnumerable<TvDbEpisodeData> episodes)
        {
            Episodes = episodes;
        }

        public IEnumerable<TvDbEpisodeData> Episodes { get; }
    }
}