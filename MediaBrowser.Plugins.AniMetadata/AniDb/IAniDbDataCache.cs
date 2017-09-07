using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb.Seiyuu;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.AniDb.Titles;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    internal interface IAniDbDataCache
    {
        IEnumerable<TitleListItemData> TitleList { get; }

        Task<Option<AniDbSeriesData>> GetSeriesAsync(int aniDbSeriesId, CancellationToken cancellationToken);

        IEnumerable<SeiyuuData> GetSeiyuu();
    }
}