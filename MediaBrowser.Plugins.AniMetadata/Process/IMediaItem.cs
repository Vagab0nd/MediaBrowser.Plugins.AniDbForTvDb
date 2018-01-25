using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal interface IMediaItem
    {
        ItemType ItemType { get; }
        
        /// <summary>
        ///     Get the data from a particular source
        /// </summary>
        Option<ISourceData> GetDataFromSource(ISource source);

        /// <summary>
        ///     Add metadata from a source to the collection of metadata for this item
        /// </summary>
        /// <param name="sourceData">The metadata to add</param>
        /// <returns>A new <see cref="IMediaItem" /> with the metadata added</returns>
        IMediaItem AddData(ISourceData sourceData);
    }
}