using System.IO;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    internal class TitlesFileSpec : AniDbFileSpec
    {
        private const string TitlesPath = "anidb\\titles";
        private readonly string _rootPath;

        public TitlesFileSpec(string rootPath)
        {
            _rootPath = rootPath;
        }

        public override string Url => "http://anidb.net/api/animetitles.xml";

        public override string DestinationFilePath => Path.Combine(_rootPath, TitlesPath, "titles.xml");

        public override bool IsGZipped => true;
    }
}