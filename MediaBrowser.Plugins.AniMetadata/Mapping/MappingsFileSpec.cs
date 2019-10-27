using System.IO;
using Emby.AniDbMetaStructure.Files;
using Emby.AniDbMetaStructure.Mapping.Data;

namespace Emby.AniDbMetaStructure.Mapping
{
    internal class MappingsFileSpec : IRemoteFileSpec<AnimeMappingListData>
    {
        private readonly string rootPath;

        public MappingsFileSpec(string rootPath)
        {
            this.rootPath = rootPath;
        }

        public string Url => "https://raw.githubusercontent.com/ScudLee/anime-lists/master/anime-list.xml";

        public string LocalPath => Path.Combine(this.rootPath, "anime-list.xml");

        public bool IsGZipped => false;
    }
}