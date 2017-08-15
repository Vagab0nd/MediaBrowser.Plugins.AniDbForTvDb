using System.Threading;
using System.Threading.Tasks;
using Functional.Maybe;
using MediaBrowser.Plugins.Anime.AniDb.Mapping.Data;

namespace MediaBrowser.Plugins.Anime.AniDb.Mapping
{
    internal interface IAnimeMappingListFactory
    {
        Task<Maybe<AnimeMappingListData>> CreateMappingListAsync(CancellationToken cancellationToken);
    }
}