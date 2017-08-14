using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Plugins.Anime.AniDb.Mapping.Data;

namespace MediaBrowser.Plugins.Anime.AniDb.Mapping
{
    internal interface IAnimeMappingListFactory
    {
        Task<AnimeMappingListData> CreateMappingListAsync(CancellationToken cancellationToken);
    }
}