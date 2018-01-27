using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    /// <summary>
    ///     Produces Emby results from <see cref="IMediaItem" />s
    /// </summary>
    internal interface IResultFactory
    {
        Either<ProcessFailedResult, IMetadataFoundResult> GetResult(IMediaItem mediaItem);
    }
}