namespace MediaBrowser.Plugins.Anime.AniDb.Mapping
{
    public class EpisodeMapping
    {
        public EpisodeMapping(int aniDbEpisodeIndex, int tvDbEpisodeIndex)
        {
            AniDbEpisodeIndex = aniDbEpisodeIndex;
            TvDbEpisodeIndex = tvDbEpisodeIndex;
        }

        public int AniDbEpisodeIndex { get; }

        public int TvDbEpisodeIndex { get; }
    }
}