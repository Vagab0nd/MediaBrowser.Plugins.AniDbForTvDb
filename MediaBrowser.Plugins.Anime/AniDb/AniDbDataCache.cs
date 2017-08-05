using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Plugins.Anime.AniDb.Data;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    internal class AniDbDataCache : IAniDbDataCache
    {
        private readonly IApplicationPaths _applicationPaths;
        private readonly IAniDbFileCache _fileCache;
        private readonly IAniDbFileParser _fileParser;
        private readonly Lazy<IEnumerable<TitleListItem>> _titleListLazy;

        public AniDbDataCache(IApplicationPaths applicationPaths, IAniDbFileCache fileCache, IAniDbFileParser fileParser)
        {
            _applicationPaths = applicationPaths;
            _fileCache = fileCache;
            _fileParser = fileParser;

            _titleListLazy = new Lazy<IEnumerable<TitleListItem>>(() =>
            {
                var fileSpec = new TitlesFileSpec(_applicationPaths.CachePath);
                var titlesFile = _fileCache.GetFileAsync(fileSpec, CancellationToken.None).Result;

                return _fileParser.ParseTitleListXml(File.ReadAllText(titlesFile.FullName)).Titles;
            });
        }

        public IEnumerable<TitleListItem> TitleList => _titleListLazy.Value;

        public async Task<AniDbSeries> GetSeriesAsync(int aniDbSeriesId, CancellationToken cancellationToken)
        {
            var fileSpec = new SeriesFileSpec(_applicationPaths.CachePath, aniDbSeriesId);

            var seriesFile = await _fileCache.GetFileAsync(fileSpec, cancellationToken);

            var series = _fileParser.ParseSeriesXml(File.ReadAllText(seriesFile.FullName));

            return series;
        }
    }
}