using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Plugins.Anime.AniDb.Seiyuu;
using MediaBrowser.Plugins.Anime.AniDb.Series;
using MediaBrowser.Plugins.Anime.AniDb.Series.Data;
using MediaBrowser.Plugins.Anime.AniDb.Titles;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    internal class AniDbDataCache : IAniDbDataCache
    {
        private readonly IApplicationPaths _applicationPaths;
        private readonly IAniDbFileCache _fileCache;
        private readonly IXmlFileParser _fileParser;
        private readonly ISeiyuuCache _seiyuuCache;
        private readonly Lazy<IEnumerable<TitleListItemData>> _titleListLazy;

        public AniDbDataCache(IApplicationPaths applicationPaths, IAniDbFileCache fileCache, IXmlFileParser fileParser,
            ISeiyuuCache seiyuuCache)
        {
            _applicationPaths = applicationPaths;
            _fileCache = fileCache;
            _fileParser = fileParser;
            _seiyuuCache = seiyuuCache;

            _titleListLazy = new Lazy<IEnumerable<TitleListItemData>>(() =>
            {
                var fileSpec = new TitlesFileSpec(_fileParser, _applicationPaths.CachePath);
                var titlesFile = _fileCache.GetFileAsync(fileSpec, CancellationToken.None).Result;

                return fileSpec.ParseFile(File.ReadAllText(titlesFile.FullName)).Titles;
            });
        }

        public IEnumerable<TitleListItemData> TitleList => _titleListLazy.Value;

        public async Task<AniDbSeriesData> GetSeriesAsync(int aniDbSeriesId, CancellationToken cancellationToken)
        {
            var fileSpec = new SeriesFileSpec(_fileParser, _applicationPaths.CachePath, aniDbSeriesId);

            var seriesFile = await _fileCache.GetFileAsync(fileSpec, cancellationToken);

            var series = fileSpec.ParseFile(File.ReadAllText(seriesFile.FullName));

            UpdateSeiyuuList(series);

            return series;
        }

        public IEnumerable<SeiyuuData> GetSeiyuu()
        {
            return _seiyuuCache.GetAll();
        }

        private void UpdateSeiyuuList(AniDbSeriesData aniDbSeriesData)
        {
            var seiyuu = aniDbSeriesData?.Characters?.Select(c => c.Seiyuu) ?? new List<SeiyuuData>();

            _seiyuuCache.Add(seiyuu);
        }
    }
}