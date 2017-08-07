using System.Threading.Tasks;
using Functional.Maybe;
using MediaBrowser.Plugins.Anime.AniDb.Data;
using MediaBrowser.Plugins.Anime.AniDb.Mapping;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    public interface IAniDbClient
    {
        Task<Maybe<AniDbSeries>> FindSeriesAsync(string title);

        Task<AniDbMapper> GetMapperAsync();

        Task<AniDbSeries> GetSeriesAsync(int aniDbSeriesId);

        Task<Maybe<AniDbSeries>> GetSeriesAsync(string aniDbSeriesIdString);
    }
}