using System.Threading;
using System.Threading.Tasks;
using Functional.Maybe;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.Mapping
{
    internal interface IAnimeMappingListFactory
    {
        Task<Maybe<IMappingList>> CreateMappingListAsync(CancellationToken cancellationToken);
    }
}