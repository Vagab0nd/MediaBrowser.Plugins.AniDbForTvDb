using Emby.AniDbMetaStructure.Process.Sources;
using Emby.AniDbMetaStructure.SourceDataLoaders;
using LanguageExt;

namespace Emby.AniDbMetaStructure.Process
{
    /// <summary>
    ///     A source of metadata
    /// </summary>
    internal interface ISource
    {
        SourceName Name { get; }

        Either<ProcessFailedResult, IEmbySourceDataLoader> GetEmbySourceDataLoader(IMediaItemType mediaItemType);

        bool ShouldUsePlaceholderSourceData(IMediaItemType mediaItemType);
    }
}