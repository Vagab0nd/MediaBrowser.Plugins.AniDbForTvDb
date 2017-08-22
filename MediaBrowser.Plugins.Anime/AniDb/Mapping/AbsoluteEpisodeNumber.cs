namespace MediaBrowser.Plugins.Anime.AniDb.Mapping
{
    public class AbsoluteEpisodeNumber
    {
        public AbsoluteEpisodeNumber(int episodeIndex)
        {
            EpisodeIndex = episodeIndex;
        }

        public int EpisodeIndex { get; }
    }
}