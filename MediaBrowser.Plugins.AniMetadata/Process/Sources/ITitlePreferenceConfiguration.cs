using MediaBrowser.Plugins.AniMetadata.Configuration;

namespace MediaBrowser.Plugins.AniMetadata.Process.Sources
{
    internal interface ITitlePreferenceConfiguration
    {
        TitleType TitlePreference { get; }
    }
}