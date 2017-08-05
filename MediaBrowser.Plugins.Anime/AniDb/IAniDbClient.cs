using System.Threading.Tasks;
using MediaBrowser.Plugins.Anime.AniDb.Data;
using MediaBrowser.Plugins.Anime.AniDb.Mapping;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    public interface IAniDbClient
    {
        Task<IOption<AniDbSeries>> FindSeriesAsync(string title);

        Task<AniDbMapper> GetMapperAsync();

        Task<AniDbSeries> GetSeriesAsync(int aniDbSeriesId);
    }
}