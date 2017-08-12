using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Plugins.Anime.AniDb.Data;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    internal interface IAniDbDataCache
    {
        IEnumerable<TitleListItem> TitleList { get; }

        Task<AniDbSeries> GetSeriesAsync(int aniDbSeriesId, CancellationToken cancellationToken);

        IEnumerable<Seiyuu> GetSeiyuu();
    }
}