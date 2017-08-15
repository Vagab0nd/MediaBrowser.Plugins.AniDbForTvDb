using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Functional.Maybe;
using MediaBrowser.Plugins.Anime.AniDb.Seiyuu;
using MediaBrowser.Plugins.Anime.AniDb.Series.Data;
using MediaBrowser.Plugins.Anime.AniDb.Titles;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    internal interface IAniDbDataCache
    {
        IEnumerable<TitleListItemData> TitleList { get; }

        Task<Maybe<AniDbSeriesData>> GetSeriesAsync(int aniDbSeriesId, CancellationToken cancellationToken);

        IEnumerable<SeiyuuData> GetSeiyuu();
    }
}