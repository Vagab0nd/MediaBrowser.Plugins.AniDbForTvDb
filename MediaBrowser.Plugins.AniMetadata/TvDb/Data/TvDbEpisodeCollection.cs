using System.Collections.Generic;

namespace MediaBrowser.Plugins.AniMetadata.TvDb.Data
{
    internal class TvDbEpisodeCollection
    {
        public TvDbEpisodeCollection(IEnumerable<TvDbEpisodeData> episodes)
        {
            Episodes = episodes ?? new List<TvDbEpisodeData>();
        }

        public IEnumerable<TvDbEpisodeData> Episodes { get; }
    }
}