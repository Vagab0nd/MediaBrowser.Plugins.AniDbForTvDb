using System.Threading.Tasks;
using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal interface IMediaItemBuilder
    {
        /// <summary>
        ///     Create a new <see cref="IMediaItem" /> by identifying the data provided by Emby
        /// </summary>
        OptionAsync<IMediaItem> IdentifyAsync(EmbyItemData embyItemData, ItemType itemType);

        /// <summary>
        ///     Enhance a <see cref="IMediaItem" /> with metadata from all sources
        /// </summary>
        /// <returns>A new <see cref="IMediaItem" /> instance with the additional metadata</returns>
        Task<IMediaItem> BuildMediaItemAsync(IMediaItem mediaItem);
    }
}