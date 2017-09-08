using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    public interface ISeriesMetadataFactory
    {
        MetadataResult<Series> NullSeriesResult { get; }

        MetadataResult<Series> CreateMetadata(AniDbSeriesData aniDbSeriesData, string metadataLanguage);
    }
}