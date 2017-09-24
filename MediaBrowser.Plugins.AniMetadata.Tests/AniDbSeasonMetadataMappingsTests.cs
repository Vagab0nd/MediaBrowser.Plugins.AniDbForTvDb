using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.MetadataMapping;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class AniDbSeasonMetadataMappingsTests
    {
        [Test]
        public void SeasonMappings_HasMappingsForAllFields()
        {
            var aniDbParser = Substitute.For<IAniDbParser>();

            var expectedMappedFields = new[]
            {
                nameof(Season.PremiereDate),
                nameof(Season.EndDate),
                nameof(Season.Name),
                nameof(Season.Overview),
                nameof(Season.CommunityRating),
                nameof(Season.Studios),
                nameof(Season.Genres),
                nameof(Season.Tags)
            };

            var aniDbSeasonMetadataMappings = new AniDbSeasonMetadataMappings(aniDbParser);

            aniDbSeasonMetadataMappings.GetSeasonMappings(1, true)
                .Select(m => m.TargetPropertyName)
                .ShouldAllBeEquivalentTo(expectedMappedFields);
        }

        [Test]
        public void SeasonMappings_MapsAllFields()
        {
            var source = new AniDbSeries(new AniDbSeriesData
            {
                StartDate = new DateTime(2017, 1, 2, 3, 4, 5),
                EndDate = new DateTime(2017, 5, 4, 3, 2, 1),
                Description = "Description",
                Ratings = new[]
                {
                    new PermanentRatingData
                    {
                        Value = 45
                    }
                }
            }, "SelectedTitle");

            var aniDbParser = Substitute.For<IAniDbParser>();

            aniDbParser.FormatDescription("Description").Returns("FormattedDescription");
            aniDbParser.GetGenres(source.Data, 1, true).Returns(new List<string> { "Genre" });
            aniDbParser.GetTags(source.Data, 1, true).Returns(new List<string> { "Tags" });
            aniDbParser.GetStudios(source.Data).Returns(new List<string> { "Studio" });

            var target = new MetadataResult<Season>
            {
                Item = new Season()
            };

            var aniDbSeasonMetadataMappings = new AniDbSeasonMetadataMappings(aniDbParser);

            aniDbSeasonMetadataMappings.GetSeasonMappings(1, true).Iter(m => m.Apply(source, target));

            target.Item.Name.Should().Be("SelectedTitle");
            target.Item.PremiereDate.Should().Be(new DateTime(2017, 1, 2, 3, 4, 5));
            target.Item.EndDate.Should().Be(new DateTime(2017, 5, 4, 3, 2, 1));
            target.Item.Overview.Should().Be("FormattedDescription");
            target.Item.Genres.Should().BeEquivalentTo("Genre");
            target.Item.Studios.Should().BeEquivalentTo("Studio");
            target.Item.Tags.Should().BeEquivalentTo("Tags");
            target.Item.CommunityRating.Should().Be(45);
        }
    }
}