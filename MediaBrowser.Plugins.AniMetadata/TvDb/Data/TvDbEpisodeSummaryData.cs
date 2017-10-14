using System;
using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.TvDb.Data
{
    public class TvDbEpisodeSummaryData
    {
        public TvDbEpisodeSummaryData(int id, string episodeName, Option<long> absoluteNumber, int airedEpisodeNumber,
            int airedSeason, int lastUpdated, DateTime firstAired, string overview)
        {
            Id = id;
            EpisodeName = episodeName;
            AbsoluteNumber = absoluteNumber;
            AiredEpisodeNumber = airedEpisodeNumber;
            AiredSeason = airedSeason;
            LastUpdated = lastUpdated;
            FirstAired = firstAired;
            Overview = overview;
        }

        public int Id { get; }

        public string EpisodeName { get; }

        public Option<long> AbsoluteNumber { get; }

        public int AiredEpisodeNumber { get; }

        public int AiredSeason { get; }

        public int LastUpdated { get; }

        public DateTime FirstAired { get; }

        public string Overview { get; }
    }
}