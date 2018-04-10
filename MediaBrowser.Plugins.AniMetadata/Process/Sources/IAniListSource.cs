using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniList.Data;

namespace MediaBrowser.Plugins.AniMetadata.Process.Sources
{
    internal interface IAniListSource : ISource
    {
        Either<ProcessFailedResult, string> SelectTitle(AniListTitleData titleData,
            string metadataLanguage, ProcessResultContext resultContext);
    }
}