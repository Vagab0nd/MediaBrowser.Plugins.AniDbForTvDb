using System.Collections.Generic;
using System.Threading.Tasks;
using Functional.Maybe;
using MediaBrowser.Plugins.Anime.AniDb.Data;
using MediaBrowser.Plugins.Anime.AniDb.Mapping;

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