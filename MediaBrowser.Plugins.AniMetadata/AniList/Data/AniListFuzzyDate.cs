using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.AniList.Data
{
    internal class AniListFuzzyDate
    {
        public AniListFuzzyDate(Option<long> year, Option<long> month, Option<long> day)
        {
            Year = year;
            Month = month;
            Day = day;
        }

        public Option<long> Year { get; }

        public Option<long> Month { get; }

        public Option<long> Day { get; }
    }
}