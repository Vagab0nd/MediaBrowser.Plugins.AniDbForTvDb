using Functional.Maybe;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.Mapping
{
    public class AbsoluteEpisodeNumber
    {
        public AbsoluteEpisodeNumber(Maybe<int> tvDbEpisodeId, int episodeIndex)
        {
            TvDbEpisodeId = tvDbEpisodeId;
            EpisodeIndex = episodeIndex;
        }

        public Maybe<int> TvDbEpisodeId { get; }

        public int EpisodeIndex { get; }
    }
}