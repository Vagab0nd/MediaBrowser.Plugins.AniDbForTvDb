using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;

namespace MediaBrowser.Plugins.AniMetadata.Providers
{
    public class AniDbOnlyEpisodeData
    {
        public AniDbOnlyEpisodeData(AniDbEpisodeData episodeData)
        {
            EpisodeData = episodeData;
        }
        
        public AniDbEpisodeData EpisodeData { get; }
    }
}