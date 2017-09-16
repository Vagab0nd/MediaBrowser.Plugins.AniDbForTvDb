using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    public interface ISeriesMetadataFactory
    {
        MetadataResult<Series> NullSeriesResult { get; }

        MetadataResult<Series> CreateMetadata(AniDbSeriesData aniDbSeriesData, string metadataLanguage);

        MetadataResult<Series> CreateMetadata(AniDbSeriesData aniDbSeriesData, TvDbSeriesData tvDbSeriesData, string metadataLanguage);
    }
}