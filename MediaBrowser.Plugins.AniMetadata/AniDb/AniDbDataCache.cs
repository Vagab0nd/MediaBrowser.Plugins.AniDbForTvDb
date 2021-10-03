﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emby.AniDbMetaStructure.AniDb.Seiyuu;
using Emby.AniDbMetaStructure.AniDb.SeriesData;
using Emby.AniDbMetaStructure.AniDb.Titles;
using Emby.AniDbMetaStructure.Files;
using LanguageExt;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Logging;

namespace Emby.AniDbMetaStructure.AniDb
{
    internal class AniDbDataCache : IAniDbDataCache
    {
        private readonly IApplicationPaths applicationPaths;
        private readonly IFileCache fileCache;
        private readonly IXmlSerialiser xmlSerializer;
        private readonly SeiyuuFileSpec seiyuuFileSpec;
        private readonly Lazy<IEnumerable<TitleListItemData>> titleListLazy;

        public AniDbDataCache(IApplicationPaths applicationPaths, IFileCache fileCache, ILogManager logManager, IXmlSerialiser xmlSerializer)
        {
            this.applicationPaths = applicationPaths;
            this.fileCache = fileCache;
            this.xmlSerializer = xmlSerializer;
            var titlesFileSpec = new TitlesFileSpec(this.applicationPaths.CachePath, this.xmlSerializer);
            this.seiyuuFileSpec = new SeiyuuFileSpec(new XmlSerialiser(logManager), this.applicationPaths.CachePath);

            this.titleListLazy = new Lazy<IEnumerable<TitleListItemData>>(() =>
            {
                var titleData = this.fileCache.GetFileContentAsync(titlesFileSpec, CancellationToken.None).Result;

                return titleData.Match(t => t.Titles, Enumerable.Empty<TitleListItemData>);
            });
        }

        public IEnumerable<TitleListItemData> TitleList => this.titleListLazy.Value;

        public async Task<Option<AniDbSeriesData>> GetSeriesAsync(int aniDbSeriesId,
            CancellationToken cancellationToken)
        {
            var fileSpec = new SeriesFileSpec(this.applicationPaths.CachePath, aniDbSeriesId, this.xmlSerializer);

            var seriesData = await this.fileCache.GetFileContentAsync(fileSpec, cancellationToken);

            seriesData.Iter(this.UpdateSeiyuuList);

            return seriesData;
        }

        public IEnumerable<SeiyuuData> GetSeiyuu()
        {
            return this.fileCache.GetFileContent(this.seiyuuFileSpec).Match(s => s.Seiyuu, Enumerable.Empty<SeiyuuData>);
        }

        private void UpdateSeiyuuList(AniDbSeriesData aniDbSeriesData)
        {
            var seriesSeiyuu = aniDbSeriesData?.Characters?.Select(c => c.Seiyuu).ToList() ?? new List<SeiyuuData>();

            if (!seriesSeiyuu.Any())
            {
                return;
            }

            var existingSeiyuu = this.GetSeiyuu();
            var newSeiyuu = seriesSeiyuu.Except(existingSeiyuu, new SeiyuuComparer());

            if (!newSeiyuu.Any())
            {
                return;
            }

            this.fileCache.SaveFile(this.seiyuuFileSpec, new SeiyuuListData
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