using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal class MetadataFoundResult<TEmbyItem> : IMetadataFoundResult<TEmbyItem> where TEmbyItem : BaseItem
    {
        public MetadataFoundResult(IMediaItem mediaItem, MetadataResult<TEmbyItem> metadataResult)
        {
            EmbyMetadataResult = metadataResult;
            MediaItem = mediaItem;
        }

        /// <summary>
        ///     The item this result is for
        /// </summary>
        public IMediaItem MediaItem { get; }

        /// <summary>
        ///     The result that can be passed back to Emby cast to the expected type
        /// </summary>
        public MetadataResult<TEmbyItem> EmbyMetadataResult { get; }
    }
}