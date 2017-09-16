using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    internal class AniDbSeries
    {
        public AniDbSeries(AniDbSeriesData data, string selectedTitle)
        {
            Data = data;
            SelectedTitle = selectedTitle;
        }

        public AniDbSeriesData Data { get; }

        public string SelectedTitle { get; }
    }
}