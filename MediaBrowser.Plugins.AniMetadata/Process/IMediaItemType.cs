using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.Configuration;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal interface IMediaItemType
    {
        MediaItemTypeValue Type { get; }

        Either<ProcessFailedResult, IMetadataFoundResult> CreateMetadataFoundResult(
            IPluginConfiguration pluginConfiguration, IMediaItem mediaItem);
    }
}