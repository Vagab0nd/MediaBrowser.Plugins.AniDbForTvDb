using LanguageExt;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal class MetadataFoundResult<TEmbyItem> : IMetadataFoundResult where TEmbyItem : BaseItem
    {
        private readonly MetadataResult<TEmbyItem> _metadataResult;

        public MetadataFoundResult(IMediaItem mediaItem, MetadataResult<TEmbyItem> metadataResult)
        {
            _metadataResult = metadataResult;
            MediaItem = mediaItem;
        }

        /// <summary>
        ///     The item this result is for
        /// </summary>
        public IMediaItem MediaItem { get; }

        /// <summary>
        ///     The result that can be passed back to Emby cast to the expected type
        /// </summary>
        public Option<MetadataResult<TItem>> GetResult<TItem>() where TItem : BaseItem
        {
            return _metadataResult as MetadataResult<TItem>;
        }
    }
}