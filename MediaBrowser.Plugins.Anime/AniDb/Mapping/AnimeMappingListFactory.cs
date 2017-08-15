using System.Threading;
using System.Threading.Tasks;
using Functional.Maybe;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Plugins.Anime.AniDb.Mapping.Data;

namespace MediaBrowser.Plugins.Anime.AniDb.Mapping
{
    internal class AnimeMappingListFactory : IAnimeMappingListFactory
    {
        private readonly IAniDbFileCache _fileCache;
        private readonly MappingsFileSpec _mappingsFileSpec;

        public AnimeMappingListFactory(IApplicationPaths applicationPaths, IAniDbFileCache fileCache)
        {
            _mappingsFileSpec = new MappingsFileSpec(applicationPaths.CachePath);
            _fileCache = fileCache;
        }

        public async Task<Maybe<AnimeMappingListData>> CreateMappingListAsync(CancellationToken cancellationToken)
        {
            var mappingList = await _fileCache.GetFileContentAsync(_mappingsFileSpec, cancellationToken);

            return mappingList;
        }
    }
}