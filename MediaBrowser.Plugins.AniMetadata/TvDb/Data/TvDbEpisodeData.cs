using System;
using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.TvDb.Data
{
    public class TvDbEpisodeData : TvDbEpisodeSummaryData
    {
        public TvDbEpisodeData(int id, string episodeName, Option<long> absoluteNumber, int airedEpisodeNumber,
            int airedSeason, int lastUpdated, DateTime firstAired, string overview, float siteRating,
            int siteRatingCount) : base(id, episodeName, absoluteNumber, airedEpisodeNumber, airedSeason, lastUpdated,
            firstAired, overview)
        {
            SiteRating = siteRating;
            SiteRatingCount = siteRatingCount;
        }

        public float SiteRating { get; }

        public int SiteRatingCount { get; }

        public override string ToString()
        {
            return $"Season {AiredSeason} Episode {AiredEpisodeNumber} '{EpisodeName}'";
        }
    }
}