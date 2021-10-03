using System.IO;
using Emby.AniDbMetaStructure.Files;
using Emby.AniDbMetaStructure.Infrastructure;
using Emby.AniDbMetaStructure.Mapping.Data;

namespace Emby.AniDbMetaStructure.Mapping
{
    internal class MappingsFileSpec : IRemoteFileSpec<AnimeMappingListData>
    {
        private readonly string rootPath;
        private readonly IXmlSerialiser serializer;

        public MappingsFileSpec(string rootPath, IXmlSerialiser serializer)
        {
            this.rootPath = rootPath;
            this.serializer = serializer;
        }

        public string Url => "https://raw.githubusercontent.com/ScudLee/anime-lists/master/anime-list.xml";

        public string LocalPath => Path.Combine(this.rootPath, "anime-list.xml");

        public bool IsGZipped => false;

        public ISerialiser Serialiser => this.serializer;
    }
}