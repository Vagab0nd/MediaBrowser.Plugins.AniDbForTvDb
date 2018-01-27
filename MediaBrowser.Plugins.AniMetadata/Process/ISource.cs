using System.Threading.Tasks;
using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    /// <summary>
    ///     A source of metadata
    /// </summary>
    internal interface ISource
    {
        string Name { get; }

        /// <summary>
        ///     Find data in this source for a single item based on existing identifiers and metadata
        /// </summary>
        Task<Either<ProcessFailedResult, ISourceData>> LookupAsync(IMediaItem mediaItem);

        /// <summary>
        ///     Find data in this source for a single item based on data from Emby
        /// </summary>
        Task<Either<ProcessFailedResult, ISourceData>> LookupAsync(EmbyItemData embyItemData);
    }
}