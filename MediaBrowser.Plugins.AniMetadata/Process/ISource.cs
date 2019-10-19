using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using MediaBrowser.Plugins.AniMetadata.SourceDataLoaders;

namespace MediaBrowser.Plugins.AniMetadata.Process
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