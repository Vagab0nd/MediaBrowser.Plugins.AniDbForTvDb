using OneOf;

namespace MediaBrowser.Plugins.AniMetadata.Providers
{
    public class EpisodeData : OneOfBase<AniDbOnlyEpisodeData, CombinedEpisodeData, NoEpisodeData>
    {
        protected EpisodeData(int index, AniDbOnlyEpisodeData value0 = null, CombinedEpisodeData value1 = null,
            NoEpisodeData value2 = null) : base(index, value0, value1, value2)
        {
        }

        public static implicit operator EpisodeData(AniDbOnlyEpisodeData value)
        {
            return new EpisodeData(0, value);
        }

        public static implicit operator EpisodeData(CombinedEpisodeData value)
        {
            return new EpisodeData(1, null, value);
        }

        public static implicit operator EpisodeData(NoEpisodeData value)
        {
            return new EpisodeData(2, null, null, value);
        }
    }
}