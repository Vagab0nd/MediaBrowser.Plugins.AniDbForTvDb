using LanguageExt;
using MediaBrowser.Controller.Providers;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    /// <summary>
    ///     Coordinates the overall process of retrieving metadata from Emby identification data
    /// </summary>
    internal interface IMediaItemProcessor
    {
        /// <summary>
        ///     Get the result containing the metadata for this media item, if any could be found
        /// </summary>
        Option<IEmbyResult> Process(ItemLookupInfo embyInfo, ItemType itemType);
    }
}