using Functional.Maybe;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.Mapping
{
    public class TvDbEpisodeNumber
    {
        public TvDbEpisodeNumber(Maybe<int> tvDbEpisodeId, int seasonIndex, int episodeIndex,
            Maybe<TvDbEpisodeNumber> followingTvDbEpisodeNumber)
        {
            TvDbEpisodeId = tvDbEpisodeId;
            SeasonIndex = seasonIndex;
            EpisodeIndex = episodeIndex;
            FollowingTvDbEpisodeNumber = followingTvDbEpisodeNumber;
        }

        public Maybe<int> TvDbEpisodeId { get; }

        public int SeasonIndex { get; }

        public int EpisodeIndex { get; }

        /// <summary>
        ///     The episode number of the episode that follows this one, if known
        /// </summary>
        public Maybe<TvDbEpisodeNumber> FollowingTvDbEpisodeNumber { get; }
    }
}