using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Plugins.Anime.Mapping.Data;

namespace MediaBrowser.Plugins.Anime.AniDb.Mapping
{
    internal class AnimeMappingListFactory
    {
        private readonly IApplicationPaths _applicationPaths;
        private readonly AniDbFileCache _fileCache;

        public AnimeMappingListFactory(IApplicationPaths applicationPaths, AniDbFileCache fileCache)
        {
            _applicationPaths = applicationPaths;
            _fileCache = fileCache;
        }

        public async Task<AnimeMappingList> CreateMappingListAsync(CancellationToken cancellationToken)
        {
            var fileSpec = new MappingsFileSpec(_applicationPaths.CachePath);
            var file = await _fileCache.GetFileAsync(fileSpec, cancellationToken);

            return ReadLocalFile(file.FullName);
        }

        private AnimeMappingList ReadLocalFile(string filePath)
        {
            var serializer = new XmlSerializer(typeof(AnimeMappingList));

            using (var stream = File.OpenRead(filePath))
            {
                return serializer.Deserialize(stream) as AnimeMappingList;
            }
        }
    }
}