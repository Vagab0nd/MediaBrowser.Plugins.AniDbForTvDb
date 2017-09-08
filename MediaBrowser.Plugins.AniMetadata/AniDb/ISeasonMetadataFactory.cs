using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    internal interface ISeasonMetadataFactory
    {
        MetadataResult<Season> NullSeasonResult { get; }

        MetadataResult<Season> CreateMetadata(AniDbSeriesData aniDbSeriesData, int seasonIndex, string metadataLanguage);
    }
}