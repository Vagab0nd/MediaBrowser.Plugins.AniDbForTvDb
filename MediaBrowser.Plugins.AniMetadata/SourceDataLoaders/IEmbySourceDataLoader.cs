using System.Threading.Tasks;
using Emby.AniDbMetaStructure.Process;
using Emby.AniDbMetaStructure.Process.Sources;
using LanguageExt;

namespace Emby.AniDbMetaStructure.SourceDataLoaders
{
    internal interface IEmbySourceDataLoader
    {
        SourceName SourceName { get; }

        bool CanLoadFrom(IMediaItemType mediaItemType);

        Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IEmbyItemData embyItemData);
    }
}