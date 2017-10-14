using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.Providers
{
    public class CombinedEpisodeData
    {
        public CombinedEpisodeData(AniDbEpisodeData aniDbEpisodeData, TvDbEpisodeData tvDbEpisodeData,
            EpisodeData followingTvDbEpisodeData)
        {
            AniDbEpisodeData = aniDbEpisodeData;
            TvDbEpisodeData = tvDbEpisodeData;
            FollowingTvDbEpisodeData = followingTvDbEpisodeData;
        }

        public AniDbEpisodeData AniDbEpisodeData { get; }

        public TvDbEpisodeData TvDbEpisodeData { get; }

        public EpisodeData FollowingTvDbEpisodeData { get; }
    }
}