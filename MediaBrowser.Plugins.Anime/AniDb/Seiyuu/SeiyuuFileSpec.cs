using MediaBrowser.Plugins.Anime.Files;

namespace MediaBrowser.Plugins.Anime.AniDb.Seiyuu
{
    internal class SeiyuuFileSpec : ILocalFileSpec<SeiyuuListData>
    {
        private readonly IXmlSerialiser _xmlSerialiser;
        private readonly string _rootPath;

        public SeiyuuFileSpec(IXmlSerialiser xmlSerialiser, string rootPath)
        {
            _xmlSerialiser = xmlSerialiser;
            _rootPath = rootPath;
        }

        public string LocalPath => System.IO.Path.Combine(_rootPath, "anidb\\seiyuu.xml");

        public ISerialiser Serialiser => _xmlSerialiser;
    }
}