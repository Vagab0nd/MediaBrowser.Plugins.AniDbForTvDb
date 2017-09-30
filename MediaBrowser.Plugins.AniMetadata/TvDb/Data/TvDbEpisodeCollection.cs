using System.Collections.Generic;

namespace MediaBrowser.Plugins.AniMetadata.TvDb.Data
{
    internal class TvDbEpisodeCollection
    {
        public TvDbEpisodeCollection(IEnumerable<TvDbEpisodeDetailData> episodes)
        {
            Episodes = episodes ?? new List<TvDbEpisodeDetailData>();
        }

        public IEnumerable<TvDbEpisodeDetailData> Episodes { get; }
    }
}