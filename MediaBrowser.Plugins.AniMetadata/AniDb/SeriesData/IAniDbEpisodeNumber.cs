namespace MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData
{
    public interface IAniDbEpisodeNumber
    {
        int Number { get; }

        EpisodeType Type { get; }
    }
}