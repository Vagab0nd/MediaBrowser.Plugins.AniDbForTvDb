using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.Mapping
{
    internal interface IAnimeMappingListFactory
    {
        Task<Option<IMappingList>> CreateMappingListAsync(CancellationToken cancellationToken);
    }
}