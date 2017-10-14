using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Mapping;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.Providers
{
    public class CombinedSeriesData
    {
        public CombinedSeriesData(SeriesIds seriesIds, AniDbSeriesData aniDbSeriesData,
            TvDbSeriesData tvDbSeriesData)
        {
            SeriesIds = seriesIds;
            AniDbSeriesData = aniDbSeriesData;
            TvDbSeriesData = tvDbSeriesData;
        }

        public SeriesIds SeriesIds { get; }

        public AniDbSeriesData AniDbSeriesData { get; }

        public TvDbSeriesData TvDbSeriesData { get; }
    }
}