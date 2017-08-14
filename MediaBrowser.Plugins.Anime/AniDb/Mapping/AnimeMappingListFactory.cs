using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Plugins.Anime.AniDb.Mapping.Data;

namespace MediaBrowser.Plugins.Anime.AniDb.Mapping
{
    internal class AnimeMappingListFactory : IAnimeMappingListFactory
    {
        private readonly IApplicationPaths _applicationPaths;
        private readonly IAniDbFileCache _fileCache;
        private readonly IXmlFileParser _fileParser;

        public AnimeMappingListFactory(IApplicationPaths applicationPaths, IAniDbFileCache fileCache,
            IXmlFileParser fileParser)
        {
            _applicationPaths = applicationPaths;
            _fileCache = fileCache;
            _fileParser = fileParser;
        }

        public async Task<AnimeMappingListData> CreateMappingListAsync(CancellationToken cancellationToken)
        {
            var fileSpec = new MappingsFileSpec(_fileParser, _applicationPaths.CachePath);
            var file = await _fileCache.GetFileAsync(fileSpec, cancellationToken);

            return fileSpec.ParseFile(File.ReadAllText(file.FullName));
        }
    }
}