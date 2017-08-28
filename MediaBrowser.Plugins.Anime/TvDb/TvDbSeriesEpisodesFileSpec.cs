using MediaBrowser.Plugins.Anime.Files;
using MediaBrowser.Plugins.Anime.TvDb.Data;

namespace MediaBrowser.Plugins.Anime.TvDb
{
    internal class TvDbSeriesEpisodesFileSpec : ILocalFileSpec<TvDbSeriesData>
    {
        private readonly string _rootPath;
        private readonly int _tvDbSeriesId;

        public TvDbSeriesEpisodesFileSpec(string rootPath, int tvDbSeriesId)
        {
            _rootPath = rootPath;
            _tvDbSeriesId = tvDbSeriesId;
        }

        public string LocalPath => System.IO.Path.Combine(_rootPath, $"anidb\\tvdb\\{_tvDbSeriesId}.xml");
    }
}