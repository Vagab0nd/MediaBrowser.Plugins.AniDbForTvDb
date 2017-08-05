using System.IO;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    internal class MappingsFileSpec : AniDbFileSpec
    {
        private readonly string _rootPath;

        public MappingsFileSpec(string rootPath)
        {
            _rootPath = rootPath;
        }

        public override string Url => "https://raw.githubusercontent.com/ScudLee/anime-lists/master/anime-list.xml";

        public override string DestinationFilePath => Path.Combine(_rootPath, "anime-list.xml");
    }
}