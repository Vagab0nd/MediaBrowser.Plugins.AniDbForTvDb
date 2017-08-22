namespace MediaBrowser.Plugins.Anime.AniDb.Mapping
{
    public class TvDbEpisodeNumber
    {
        public TvDbEpisodeNumber(int seasonIndex, int episodeIndex)
        {
            SeasonIndex = seasonIndex;
            EpisodeIndex = episodeIndex;
        }

        public int SeasonIndex { get; }

        public int EpisodeIndex { get; }
    }
}