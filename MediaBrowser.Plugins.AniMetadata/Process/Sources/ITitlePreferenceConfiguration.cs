using Emby.AniDbMetaStructure.Configuration;

namespace Emby.AniDbMetaStructure.Process.Sources
{
    internal interface ITitlePreferenceConfiguration
    {
        TitleType TitlePreference { get; }
    }
}