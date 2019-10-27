using System.Collections.Generic;

namespace Emby.AniDbMetaStructure.TvDb.Data
{
    internal class TvDbEpisodeCollection
    {
        public TvDbEpisodeCollection(IEnumerable<TvDbEpisodeData> episodes)
        {
            this.Episodes = episodes ?? new List<TvDbEpisodeData>();
        }

        public IEnumerable<TvDbEpisodeData> Episodes { get; }
    }
}