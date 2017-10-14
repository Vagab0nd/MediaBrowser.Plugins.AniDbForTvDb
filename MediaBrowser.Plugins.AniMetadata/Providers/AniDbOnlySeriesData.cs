using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Mapping;

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