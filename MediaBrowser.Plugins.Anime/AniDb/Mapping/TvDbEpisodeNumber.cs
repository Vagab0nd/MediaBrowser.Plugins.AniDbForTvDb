using Functional.Maybe;

namespace MediaBrowser.Plugins.Anime.AniDb.Mapping
{
    public class TvDbEpisodeNumber
    {
        public TvDbEpisodeNumber(int seasonIndex, int episodeIndex, Maybe<TvDbEpisodeNumber> followingTvDbEpisodeNumber)
        {
            SeasonIndex = seasonIndex;
            EpisodeIndex = episodeIndex;
            FollowingTvDbEpisodeNumber = followingTvDbEpisodeNumber;
        }

        public int SeasonIndex { get; }

        public int EpisodeIndex { get; }

        /// <summary>
        ///     The episode number of the episode that follows this one, if known
        /// </summary>
        public Maybe<TvDbEpisodeNumber> FollowingTvDbEpisodeNumber { get; }
    }
}