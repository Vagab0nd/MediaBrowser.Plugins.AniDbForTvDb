using System;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal class MetadataFoundResult<TMediaItem> where TMediaItem : BaseItem
    {
        private readonly MetadataResult<TMediaItem> _metadataResult;

        public MetadataFoundResult(IMediaItem mediaItem, MetadataResult<TMediaItem> metadataResult)
        {
            _metadataResult = metadataResult;
            MediaItem = mediaItem;
        }

        /// <summary>
        /// The item this result is for
        /// </summary>
        IMediaItem MediaItem { get; }

        /// <summary>
        /// The result that can be passed back to Emby cast to the expected type
        /// </summary>
        MetadataResult<TItem> GetResult<TItem>() where TItem : BaseItem
        {
            throw new NotImplementedException();
        }
    }
}