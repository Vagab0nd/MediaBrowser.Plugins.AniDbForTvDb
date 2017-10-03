using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    internal interface ISeasonMetadataFactory
    {
        MetadataResult<Season> NullResult { get; }

        MetadataResult<Season> CreateMetadata(AniDbSeriesData aniDbSeriesData, int seasonIndex,
            string metadataLanguage);

        MetadataResult<Season> CreateMetadata(AniDbSeriesData aniDbSeriesData, TvDbSeriesData tvDbSeriesData,
            int seasonIndex, string metadataLanguage);
    }
}