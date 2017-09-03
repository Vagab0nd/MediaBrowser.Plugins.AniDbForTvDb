using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.TvDb.Data
{
    public class TvDbEpisodeData
    {
        public TvDbEpisodeData(int id, string episodeName, Option<long> absoluteNumber, int airedEpisodeNumber,
            int airedSeason, int lastUpdated)
        {
            Id = id;
            EpisodeName = episodeName;
            AbsoluteNumber = absoluteNumber;
            AiredEpisodeNumber = airedEpisodeNumber;
            AiredSeason = airedSeason;
            LastUpdated = lastUpdated;
        }

        public int Id { get; }

        public string EpisodeName { get; }

        public Option<long> AbsoluteNumber { get; }

        public int AiredEpisodeNumber { get; }

        public int AiredSeason { get; }

        public int LastUpdated { get; }
    }
}