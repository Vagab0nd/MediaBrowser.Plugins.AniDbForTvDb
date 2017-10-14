using System.IO;
using MediaBrowser.Plugins.AniMetadata.Files;
using MediaBrowser.Plugins.AniMetadata.Mapping.Data;

namespace MediaBrowser.Plugins.AniMetadata.Mapping
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