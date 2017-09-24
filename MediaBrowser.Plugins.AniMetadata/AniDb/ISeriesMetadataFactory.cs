using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    public interface ISeriesMetadataFactory
    {
        MetadataResult<Series> NullResult { get; }

        MetadataResult<Series> CreateMetadata(AniDbSeriesData aniDbSeriesData);

        MetadataResult<Series> CreateMetadata(AniDbSeriesData aniDbSeriesData, TvDbSeriesData tvDbSeriesData);
    }
}