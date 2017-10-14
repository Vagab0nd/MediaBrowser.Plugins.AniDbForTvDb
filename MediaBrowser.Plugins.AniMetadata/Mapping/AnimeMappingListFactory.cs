using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Plugins.AniMetadata.Files;

namespace MediaBrowser.Plugins.AniMetadata.Mapping
{
    internal class AnimeMappingListFactory : IAnimeMappingListFactory
    {
        private readonly IFileCache _fileCache;
        private readonly MappingsFileSpec _mappingsFileSpec;

        public AnimeMappingListFactory(IApplicationPaths applicationPaths, IFileCache fileCache)
        {
            _mappingsFileSpec = new MappingsFileSpec(applicationPaths.CachePath);
            _fileCache = fileCache;
        }

        public async Task<Option<IMappingList>> CreateMappingListAsync(CancellationToken cancellationToken)
        {
            var mappingList = await _fileCache.GetFileContentAsync(_mappingsFileSpec, cancellationToken);

            return mappingList.Match(m => MappingList.FromData(m).Select(ml => ml as IMappingList),
                () => Option<IMappingList>.None);
        }
    }
}