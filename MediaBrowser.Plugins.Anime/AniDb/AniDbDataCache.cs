using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Plugins.Anime.AniDb.Data;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    internal class AniDbDataCache
    {
        private readonly AniDbFileCache _fileCache;
        private readonly AniDbFileParser _fileParser;
        private readonly IHttpClient _httpClient;
        private readonly Lazy<IEnumerable<TitleListItem>> _titleListLazy;

        public AniDbDataCache(AniDbFileCache fileCache, AniDbFileParser fileParser, IHttpClient httpClient)
        {
            _fileCache = fileCache;
            _fileParser = fileParser;
            _httpClient = httpClient;

            _titleListLazy = new Lazy<IEnumerable<TitleListItem>>(() =>
            {
                var titlesFile = _fileCache.GetTitlesFileAsync(httpClient, CancellationToken.None).Result;

                return _fileParser.ParseTitleListXml(File.ReadAllText(titlesFile.FullName)).Titles;
            });
        }

        public IEnumerable<TitleListItem> TitleList => _titleListLazy.Value;

        public async Task<AniDbSeries> GetSeriesAsync(int aniDbSeriesId, CancellationToken cancellationToken)
        {
            var seriesFile = await _fileCache.GetSeriesFileAsync(aniDbSeriesId, _httpClient, cancellationToken);

            var series = _fileParser.ParseSeriesXml(File.ReadAllText(seriesFile.FullName));

            return series;
        }
    }
}