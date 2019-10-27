using System.Threading.Tasks;
using Emby.AniDbMetaStructure.Process;
using LanguageExt;

namespace Emby.AniDbMetaStructure.SourceDataLoaders
{
    internal interface ISourceDataLoader
    {
        bool CanLoadFrom(object sourceData);

        Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IMediaItem mediaItem, object sourceData);
    }
}