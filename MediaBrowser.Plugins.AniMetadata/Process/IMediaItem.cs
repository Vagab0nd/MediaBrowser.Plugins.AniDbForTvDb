using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal interface IMediaItem
    {
        ItemType ItemType { get; }
        
        /// <summary>
        ///     Determine whether this item has metadata from a particular source
        /// </summary>
        bool HasMetadataFromSource(ISource source);

        /// <summary>
        ///     Get the data from a particular source
        /// </summary>
        Option<ISourceData> GetMetadataFromSource(ISource source);

        /// <summary>
        ///     Add metadata from a source to the collection of metadata for this item
        /// </summary>
        /// <param name="metadata">The metadata to add</param>
        /// <returns>A new <see cref="IMediaItem" /> with the metadata added</returns>
        IMediaItem AddMetadata(ISourceData metadata);
    }
}