using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;

namespace MediaBrowser.Plugins.AniMetadata.SourceDataLoaders
{
    internal interface IEmbySourceDataLoader
    {
        SourceName SourceName { get; }

        bool CanLoadFrom(IMediaItemType mediaItemType);

        Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IEmbyItemData embyItemData);
    }
}