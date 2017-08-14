using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Plugins.Anime.AniDb.Data;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    internal interface IAniDbDataCache
    {
        IEnumerable<TitleListItemData> TitleList { get; }

        Task<AniDbSeriesData> GetSeriesAsync(int aniDbSeriesId, CancellationToken cancellationToken);

        IEnumerable<SeiyuuData> GetSeiyuu();
    }
}