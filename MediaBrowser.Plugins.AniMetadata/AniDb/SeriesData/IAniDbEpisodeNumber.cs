namespace MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData
{
    public interface IAniDbEpisodeNumber
    {
        int Number { get; }

        int SeasonNumber { get; }

        EpisodeType Type { get; }
    }
}