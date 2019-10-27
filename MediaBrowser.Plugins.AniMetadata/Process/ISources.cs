using Emby.AniDbMetaStructure.Process.Sources;

namespace Emby.AniDbMetaStructure.Process
{
    internal interface ISources
    {
        IAniDbSource AniDb { get; }

        ITvDbSource TvDb { get; }

        IAniListSource AniList { get; }

        ISource Get(string sourceName);
    }
}