namespace MediaBrowser.Plugins.AniMetadata.Mapping
{
    public class SpecialEpisodePosition
    {
        public SpecialEpisodePosition(int specialEpisodeIndex, int followingStandardEpisodeIndex)
        {
            SpecialEpisodeIndex = specialEpisodeIndex;
            FollowingStandardEpisodeIndex = followingStandardEpisodeIndex;
        }

        public int SpecialEpisodeIndex { get; }

        public int FollowingStandardEpisodeIndex { get; }
    }
}