using System.Collections.Generic;
using System.Threading.Tasks;
using Functional.Maybe;
using MediaBrowser.Plugins.Anime.AniDb.Mapping;
using MediaBrowser.Plugins.Anime.AniDb.Seiyuu;
using MediaBrowser.Plugins.Anime.AniDb.Series;
using MediaBrowser.Plugins.Anime.AniDb.Series.Data;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    public interface IAniDbClient
    {
        Task<Maybe<AniDbSeriesData>> FindSeriesAsync(string title);

        Task<AniDbMapper> GetMapperAsync();

        Task<AniDbSeriesData> GetSeriesAsync(int aniDbSeriesId);

        Task<Maybe<AniDbSeriesData>> GetSeriesAsync(string aniDbSeriesIdString);

        IEnumerable<SeiyuuData> FindSeiyuu(string name);

        Maybe<SeiyuuData> GetSeiyuu(int seiyuuId);
    }
}