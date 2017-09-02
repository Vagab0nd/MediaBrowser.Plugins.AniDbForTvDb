using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Functional.Maybe;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Plugins.AniMetadata.AniDb.Seiyuu;
using MediaBrowser.Plugins.AniMetadata.AniDb.Series.Data;
using MediaBrowser.Plugins.AniMetadata.AniDb.Titles;
using MediaBrowser.Plugins.AniMetadata.Files;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    internal class AniDbDataCache : IAniDbDataCache
    {
        private readonly IApplicationPaths _applicationPaths;
        private readonly IFileCache _fileCache;
        private readonly SeiyuuFileSpec _seiyuuFileSpec;
        private readonly Lazy<IEnumerable<TitleListItemData>> _titleListLazy;

        public AniDbDataCache(IApplicationPaths applicationPaths, IFileCache fileCache)
        {
            _applicationPaths = applicationPaths;
            _fileCache = fileCache;
            var titlesFileSpec = new TitlesFileSpec(_applicationPaths.CachePath);
            _seiyuuFileSpec = new SeiyuuFileSpec(new XmlSerialiser(), _applicationPaths.CachePath);

            _titleListLazy = new Lazy<IEnumerable<TitleListItemData>>(() =>
            {
                var titleData = _fileCache.GetFileContentAsync(titlesFileSpec, CancellationToken.None).Result;

                return titleData.SelectOrElse(t => t.Titles, Enumerable.Empty<TitleListItemData>);
            });
        }

        public IEnumerable<TitleListItemData> TitleList => _titleListLazy.Value;

        public async Task<Maybe<AniDbSeriesData>> GetSeriesAsync(int aniDbSeriesId, CancellationToken cancellationToken)
        {
            var fileSpec = new SeriesFileSpec(_applicationPaths.CachePath, aniDbSeriesId);

            var seriesData = await _fileCache.GetFileContentAsync(fileSpec, cancellationToken);

            seriesData.Do(UpdateSeiyuuList);

            return seriesData;
        }

        public IEnumerable<SeiyuuData> GetSeiyuu()
        {
            return _fileCache.GetFileContent(_seiyuuFileSpec).SelectOrElse(s => s.Seiyuu, Enumerable.Empty<SeiyuuData>);
        }

        private void UpdateSeiyuuList(AniDbSeriesData aniDbSeriesData)
        {
            var seriesSeiyuu = aniDbSeriesData?.Characters?.Select(c => c.Seiyuu).ToList() ?? new List<SeiyuuData>();

            if (!seriesSeiyuu.Any())
            {
                return;
            }

            var existingSeiyuu = GetSeiyuu();
            var newSeiyuu = seriesSeiyuu.Except(existingSeiyuu, new SeiyuuComparer());

            if (!newSeiyuu.Any())
            {
                return;
            }

            _fileCache.SaveFile(_seiyuuFileSpec, new SeiyuuListData
            {
                Seiyuu = existingSeiyuu.Concat(newSeiyuu).ToArray()
            });
        }

        private class SeiyuuComparer : IEqualityComparer<SeiyuuData>
        {
            public bool Equals(SeiyuuData x, SeiyuuData y)
            {
                return x != null && y != null && x.Id == y.Id;
            }

            public int GetHashCode(SeiyuuData obj)
            {
                return obj.Id.GetHashCode();
            }
        }
    }
}