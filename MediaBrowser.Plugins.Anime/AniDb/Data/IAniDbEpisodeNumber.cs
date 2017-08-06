namespace MediaBrowser.Plugins.Anime.AniDb.Data
{
    public interface IAniDbEpisodeNumber
    {
        int Number { get; }

        EpisodeType Type { get; }
    }
}