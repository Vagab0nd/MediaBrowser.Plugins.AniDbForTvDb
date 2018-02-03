using System;
using System.Collections.Generic;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.Tests.TestData
{
    public static class TvDbTestData
    {
        public static TvDbSeriesData Series(int id)
        {
            return new TvDbSeriesData(id, "", new DateTime(2017, 1, 2), "", 0, AirDay.Friday, "", 0,
                new List<string>(), new List<string>(), "");
        }

        public static TvDbEpisodeData Episode(int id, int episodeIndex = 0, int seasonIndex = 0,
            long? absoluteEpisodeIndex = null, string name = "")
        {
            return new TvDbEpisodeData(id, name, absoluteEpisodeIndex.ToOption(), episodeIndex, seasonIndex, 0,
                new DateTime(2017, 1, 2), "", 0, 0);
        }
    }
}