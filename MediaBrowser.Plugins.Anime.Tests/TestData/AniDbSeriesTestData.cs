using MediaBrowser.Plugins.Anime.AniDb.Data;

namespace MediaBrowser.Plugins.Anime.Tests.TestData
{
    internal static class AniDbSeriesTestData
    {
        public static AniDbSeries WithStandardData(this AniDbSeries aniDbSeries)
        {
            return new AniDbSeries
            {
                Id = 324,
                Description = "Series description",
                Titles = new[]
                {
                    new ItemTitle
                    {
                        Language="en",
                        Title = "EnTitle",
                        Type = "official"
                    },
                },
                Ratings = new Rating[]
                {
                    new PermanentRating
                    {
                        Value = 6.53f,
                        VoteCount=41
                    }
                }
            };
        }

        public static AniDbSeries WithoutTags(this AniDbSeries aniDbSeries)
        {
            aniDbSeries = aniDbSeries.WithStandardData();

            aniDbSeries.Tags = null;

            return aniDbSeries;
        }
    }
}