using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;

namespace MediaBrowser.Plugins.AniMetadata.Providers
{
    public class AniDbOnlySeriesData
    {
        public AniDbOnlySeriesData(SeriesIds seriesIds, AniDbSeriesData aniDbSeriesData)
        {
            SeriesIds = seriesIds;
            AniDbSeriesData = aniDbSeriesData;
        }

        public SeriesIds SeriesIds { get; }

        public AniDbSeriesData AniDbSeriesData { get; }
    }
}