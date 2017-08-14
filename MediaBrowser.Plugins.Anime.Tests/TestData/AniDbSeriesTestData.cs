using MediaBrowser.Plugins.Anime.AniDb.Series;
using MediaBrowser.Plugins.Anime.AniDb.Series.Data;

namespace MediaBrowser.Plugins.Anime.Tests.TestData
{
    internal static class AniDbSeriesTestData
    {
        public static AniDbSeriesData WithStandardData(this AniDbSeriesData aniDbSeriesData)
        {
            return new AniDbSeriesData
            {
                Id = 324,
                Description = "Series description",
                Titles = new[]
                {
                    new ItemTitleData
                    {
                        Language="en",
                        Title = "EnTitle",
                        Type = "official"
                    },
                },
                Ratings = new RatingData[]
                {
                    new PermanentRatingData
                    {
                        Value = 6.53f,
                        VoteCount=41
                    }
                },
                Creators = new CreatorData[0]
            };
        }

        public static AniDbSeriesData WithoutTags(this AniDbSeriesData aniDbSeriesData)
        {
            aniDbSeriesData = aniDbSeriesData.WithStandardData();

            aniDbSeriesData.Tags = null;

            return aniDbSeriesData;
        }
    }
}