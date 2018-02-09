namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal interface ISources
    {
        ISource AniDb { get; }

        ISource TvDb { get; }

        ISource Get(string sourceName);
    }
}