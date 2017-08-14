using System.IO;
using MediaBrowser.Plugins.Anime.AniDb.Mapping.Data;

namespace MediaBrowser.Plugins.Anime.AniDb.Mapping
{
    internal class MappingsFileSpec : AniDbFileSpec<AnimeMappingListData>
    {
        private readonly string _rootPath;

        public MappingsFileSpec(IXmlFileParser xmlFileParser, string rootPath) : base(xmlFileParser)
        {
            _rootPath = rootPath;
        }

        public override string Url => "https://raw.githubusercontent.com/ScudLee/anime-lists/master/anime-list.xml";

        public override string DestinationFilePath => Path.Combine(_rootPath, "anime-list.xml");

        public override bool IsGZipped => false;
    }
}