using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.Anime.AniDb.Data;

namespace MediaBrowser.Plugins.Anime.Providers.AniDb2
{
    public interface IEmbyMetadataFactory
    {
        MetadataResult<Series> CreateSeriesMetadataResult(AniDbSeries aniDbSeries, string metadataLanguage);

        MetadataResult<Season> CreateSeasonMetadataResult(AniDbSeries aniDbSeries, int seasonIndex, string metadataLanguage);
    }
}