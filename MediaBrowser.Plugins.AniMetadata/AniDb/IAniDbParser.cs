using System.Collections.Generic;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    internal interface IAniDbParser
    {
        string FormatDescription(string description);

        IEnumerable<string> GetGenres(AniDbSeriesData aniDbSeriesData);

        IEnumerable<string> GetStudios(AniDbSeriesData aniDbSeriesData);

        IEnumerable<string> GetTags(AniDbSeriesData aniDbSeriesData);

        IEnumerable<PersonInfo> GetPeople(AniDbSeriesData aniDbSeriesData);
    }
}