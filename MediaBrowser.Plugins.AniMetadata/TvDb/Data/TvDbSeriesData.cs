using System;
using System.Collections.Generic;
using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.TvDb.Data
{
    public class TvDbSeriesData
    {
        public TvDbSeriesData(int id, string seriesName, Option<DateTime> firstAired, string network, int runtime,
            Option<AirDay> airsDayOfWeek, string airsTime, float siteRating, IEnumerable<string> aliases,
            IEnumerable<string> genre, string overview)
        {
            Id = id;
            SeriesName = seriesName;
            FirstAired = firstAired;
            Network = network;
            Runtime = runtime;
            AirsDayOfWeek = airsDayOfWeek;
            AirsTime = airsTime;
            SiteRating = siteRating;
            Aliases = aliases ?? new List<string>();
            Genres = genre ?? new List<string>();
            Overview = overview;
        }

        public int Id { get; }

        public string SeriesName { get; }

        public Option<DateTime> FirstAired { get; }

        public string Network { get; }

        public int Runtime { get; }

        public Option<AirDay> AirsDayOfWeek { get; }

        public string AirsTime { get; }

        public float SiteRating { get; }

        public IEnumerable<string> Aliases { get; }

        public IEnumerable<string> Genres { get; }

        public string Overview { get; }
    }
}