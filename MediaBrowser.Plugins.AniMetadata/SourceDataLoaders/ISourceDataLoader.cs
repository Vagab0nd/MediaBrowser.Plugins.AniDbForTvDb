using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.Process;

namespace MediaBrowser.Plugins.AniMetadata.SourceDataLoaders
{
    internal interface ISourceDataLoader
    {
        bool CanLoadFrom(object sourceData);

        Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IMediaItem mediaItem, object sourceData);
    }
}