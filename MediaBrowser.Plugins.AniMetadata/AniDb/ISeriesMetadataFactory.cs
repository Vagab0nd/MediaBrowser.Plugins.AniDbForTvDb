using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    /// <summary>
    ///     Creates Emby <see cref="MetadataResult{T}" /> instances based on source data
    /// </summary>
    public interface ISeriesMetadataFactory
    {
        /// <summary>
        ///     The result returned when no result could be created
        /// </summary>
        MetadataResult<Series> NullResult { get; }

        /// <summary>
        ///     Create a result based solely on AniDb data
        /// </summary>
        MetadataResult<Series> CreateMetadata(AniDbSeriesData aniDbSeriesData, string metadataLanguage);

        /// <summary>
        ///     Create a result based on a combination of AniDb and TvDb data
        /// </summary>
        MetadataResult<Series> CreateMetadata(AniDbSeriesData aniDbSeriesData, TvDbSeriesData tvDbSeriesData, string metadataLanguage);
    }
}