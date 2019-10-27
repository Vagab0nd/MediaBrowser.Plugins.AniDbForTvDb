using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Emby.AniDbMetaStructure.AniDb.Seiyuu;
using Emby.AniDbMetaStructure.AniDb.SeriesData;
using Emby.AniDbMetaStructure.AniDb.Titles;
using LanguageExt;

namespace Emby.AniDbMetaStructure.AniDb
{
    internal interface IAniDbDataCache
    {
        IEnumerable<TitleListItemData> TitleList { get; }

        Task<Option<AniDbSeriesData>> GetSeriesAsync(int aniDbSeriesId, CancellationToken cancellationToken);

        IEnumerable<SeiyuuData> GetSeiyuu();
    }
}