using Emby.AniDbMetaStructure.AniList.Data;
using LanguageExt;

namespace Emby.AniDbMetaStructure.Process.Sources
{
    internal interface IAniListSource : ISource
    {
        Either<ProcessFailedResult, string> SelectTitle(AniListTitleData titleData,
            string metadataLanguage, ProcessResultContext resultContext);
    }
}