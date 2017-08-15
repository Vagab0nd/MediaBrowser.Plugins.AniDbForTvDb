namespace MediaBrowser.Plugins.Anime.AniDb.Seiyuu
{
    internal class SeiyuuFileSpec : ILocalFileSpec<SeiyuuListData>
    {
        private readonly string _rootPath;

        public SeiyuuFileSpec(string rootPath)
        {
            _rootPath = rootPath;
        }

        public string LocalPath => System.IO.Path.Combine(_rootPath, "anidb\\seiyuu.xml");
    }
}