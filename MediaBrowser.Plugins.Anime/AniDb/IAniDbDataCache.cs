using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Functional.Maybe;
using MediaBrowser.Plugins.AniMetadata.AniDb.Seiyuu;
using MediaBrowser.Plugins.AniMetadata.AniDb.Series.Data;
using MediaBrowser.Plugins.AniMetadata.AniDb.Titles;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    internal interface IAniDbDataCache
    {
        IEnumerable<TitleListItemData> TitleList { get; }

        Task<Maybe<AniDbSeriesData>> GetSeriesAsync(int aniDbSeriesId, CancellationToken cancellationToken);

        IEnumerable<SeiyuuData> GetSeiyuu();
    }
}