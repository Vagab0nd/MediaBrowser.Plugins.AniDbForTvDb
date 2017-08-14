namespace MediaBrowser.Plugins.Anime.AniDb.Series.Data
{
    public interface IAniDbEpisodeNumber
    {
        int Number { get; }

        EpisodeType Type { get; }
    }
}