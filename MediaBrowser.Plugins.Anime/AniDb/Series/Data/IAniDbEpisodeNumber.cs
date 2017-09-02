namespace MediaBrowser.Plugins.AniMetadata.AniDb.Series.Data
{
    public interface IAniDbEpisodeNumber
    {
        int Number { get; }

        EpisodeType Type { get; }
    }
}