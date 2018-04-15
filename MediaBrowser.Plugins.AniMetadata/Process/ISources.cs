using MediaBrowser.Plugins.AniMetadata.Process.Sources;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal interface ISources
    {
        IAniDbSource AniDb { get; }

        ITvDbSource TvDb { get; }

        IAniListSource AniList { get; }

        ISource Get(string sourceName);
    }
}