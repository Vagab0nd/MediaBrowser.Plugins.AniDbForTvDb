using System.IO;
using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping.Data;
using MediaBrowser.Plugins.AniMetadata.Files;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.Mapping
{
    internal class MappingsFileSpec : IRemoteFileSpec<AnimeMappingListData>
    {
        private readonly string _rootPath;

        public MappingsFileSpec(string rootPath)
        {
            _rootPath = rootPath;
        }

        public string Url => "https://raw.githubusercontent.com/ScudLee/anime-lists/master/anime-list.xml";

        public string LocalPath => Path.Combine(_rootPath, "anime-list.xml");

        public bool IsGZipped => false;
    }
}