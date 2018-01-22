using System;
using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal class MediaItem : IMediaItem
    {
        /// <summary>
        /// Create a new <see cref="MediaItem"/>
        /// </summary>
        /// <param name="itemType">The type of the media item</param>
        /// <param name="metadata">The metadata from the source used to initially identify this media item</param>
        public MediaItem(ItemType itemType, ISourceData metadata)
        {
            ItemType = itemType;
        }

        public ItemType ItemType { get; }
        
        public bool HasMetadataFromSource(ISource source)
        {
            throw new NotImplementedException();
        }

        public Option<ISourceData> GetMetadataFromSource(ISource source)
        {
            throw new NotImplementedException();
        }

        public IMediaItem AddMetadata(ISourceData metadata)
        {
            throw new NotImplementedException();
        }
    }
}