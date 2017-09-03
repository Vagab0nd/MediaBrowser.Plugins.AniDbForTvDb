using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.Mapping
{
    public class AbsoluteEpisodeNumber
    {
        public AbsoluteEpisodeNumber(Option<int> tvDbEpisodeId, int episodeIndex)
        {
            TvDbEpisodeId = tvDbEpisodeId;
            EpisodeIndex = episodeIndex;
        }

        public Option<int> TvDbEpisodeId { get; }

        public int EpisodeIndex { get; }
    }
}