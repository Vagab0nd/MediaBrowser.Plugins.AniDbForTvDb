using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;

namespace MediaBrowser.Plugins.AniMetadata.Providers
{
    public class SeriesData
    {
        public SeriesData(SeriesIds seriesIds, AniDbSeriesData aniDbSeriesData)
        {
            SeriesIds = seriesIds;
            AniDbSeriesData = aniDbSeriesData;
        }

        public SeriesIds SeriesIds { get; }

        public AniDbSeriesData AniDbSeriesData { get; }
    }
}