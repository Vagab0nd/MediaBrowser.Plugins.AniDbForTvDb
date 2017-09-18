using System;
using System.Linq;
using FluentAssertions;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.MetadataMapping;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class AniDbEpisodeMetadataMappingsTests
    {
        [Test]
        public void EpisodeMappings_HasMappingsForAllFields()
        {
            var expectedMappedFields = new[]
            {
                nameof(Episode.PremiereDate),
                nameof(Episode.RunTimeTicks),
                nameof(Episode.Name),
                nameof(Episode.Overview),
                nameof(Episode.CommunityRating)
            };

            var aniDbEpisodeMetadataMappings = new AniDbEpisodeMetadataMappings();

            aniDbEpisodeMetadataMappings.EpisodeMappings.Select(m => m.TargetPropertyName)
                .ShouldAllBeEquivalentTo(expectedMappedFields);
        }

        [Test]
        public void EpisodeMappings_MapsAllFields()
        {
            var source = new AniDbEpisode(new AniDbEpisodeData
            {
                AirDate = new DateTime(2017, 1, 2, 3, 4, 5),
                TotalMinutes = 35,
                Summary = "Description",
                Rating = new EpisodeRatingData { Rating = 45 }
            }, "SelectedTitle");

            var target = new MetadataResult<Episode>
            {
                Item = new Episode()
            };

            var aniDbEpisodeMetadataMappings = new AniDbEpisodeMetadataMappings();

            aniDbEpisodeMetadataMappings.EpisodeMappings.Iter(m => m.Apply(source, target));

            target.Item.Name.Should().Be("SelectedTitle");
            target.Item.PremiereDate.Should().Be(new DateTime(2017, 1, 2, 3, 4, 5));
            target.Item.RunTimeTicks.Should().Be(21000000000L);
            target.Item.Overview.Should().Be("Description");
            target.Item.CommunityRating.Should().Be(45);
        }
    }
}