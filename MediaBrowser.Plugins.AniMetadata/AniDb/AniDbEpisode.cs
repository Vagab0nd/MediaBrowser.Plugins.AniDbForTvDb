using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    internal class AniDbEpisode
    {
        public AniDbEpisode(AniDbEpisodeData data, string selectedTitle)
        {
            Data = data;
            SelectedTitle = selectedTitle;
        }

        public AniDbEpisodeData Data { get; }

        public string SelectedTitle { get; }
    }
}