using LanguageExt;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Plugins.AniMetadata.Configuration;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal interface IMediaItemType
    {
        MediaItemTypeValue Type { get; }
    }

    internal interface IMediaItemType<TEmbyItem> : IMediaItemType where TEmbyItem : BaseItem
    {
        Either<ProcessFailedResult, IMetadataFoundResult<TEmbyItem>> CreateMetadataFoundResult(
            IPluginConfiguration pluginConfiguration, IMediaItem mediaItem);
    }
}