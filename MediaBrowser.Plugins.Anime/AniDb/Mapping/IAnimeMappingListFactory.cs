using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Plugins.Anime.Mapping.Data;

namespace MediaBrowser.Plugins.Anime.AniDb.Mapping
{
    internal interface IAnimeMappingListFactory
    {
        Task<AnimeMappingList> CreateMappingListAsync(CancellationToken cancellationToken);
    }
}