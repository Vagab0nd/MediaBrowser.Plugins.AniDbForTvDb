using System.Threading;
using System.Threading.Tasks;
using Functional.Maybe;
using MediaBrowser.Common.Configuration;

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

        public async Task<Maybe<IMappingList>> CreateMappingListAsync(CancellationToken cancellationToken)
        {
            var mappingList = await _fileCache.GetFileContentAsync(_mappingsFileSpec, cancellationToken);

            return mappingList.Select(m => MappingList.FromData(m).Select(ml => ml as IMappingList)).Collapse();
        }
    }
}