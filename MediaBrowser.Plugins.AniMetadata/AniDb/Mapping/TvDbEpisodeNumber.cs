using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.Mapping
{
    public class TvDbEpisodeNumber
    {
        public TvDbEpisodeNumber(Option<int> tvDbEpisodeId, int seasonIndex, int episodeIndex,
            Option<TvDbEpisodeNumber> followingTvDbEpisodeNumber)
        {
            TvDbEpisodeId = tvDbEpisodeId;
            SeasonIndex = seasonIndex;
            EpisodeIndex = episodeIndex;
            FollowingTvDbEpisodeNumber = followingTvDbEpisodeNumber;
        }

        public Option<int> TvDbEpisodeId { get; }

        public int SeasonIndex { get; }

        public int EpisodeIndex { get; }

        /// <summary>
        ///     The episode number of the episode that follows this one, if known
        /// </summary>
        public Option<TvDbEpisodeNumber> FollowingTvDbEpisodeNumber { get; }
    }
}