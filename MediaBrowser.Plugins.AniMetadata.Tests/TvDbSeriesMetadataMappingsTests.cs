using System;
using System.Linq;
using FluentAssertions;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;
using MediaBrowser.Plugins.AniMetadata.TvDb.MetadataMapping;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class TvDbSeriesMetadataMappingsTests
    {
        [Test]
        public void SeriesMappings_HasMappingsForAllFields()
        {
            var expectedMappedFields = new[]
            {
                nameof(Series.PremiereDate),
                nameof(Series.Name),
                nameof(Series.Overview),
                nameof(Series.CommunityRating),
                nameof(Series.Genres),
                nameof(Series.Tags),
                nameof(Series.AirDays),
                nameof(Series.AirTime)
            };

            var aniDbSeriesMetadataMappings = new TvDbSeriesMetadataMappings(new PluginConfiguration());

            aniDbSeriesMetadataMappings.SeriesMappings.Select(m => m.TargetPropertyName)
                .ShouldAllBeEquivalentTo(expectedMappedFields);
        }

        [Test]
        public void SeriesMappings_MapsAllFields()
        {
            var source = new TvDbSeriesData(1, "SeriesName", new DateTime(2017, 1, 2, 3, 4, 5), "Network", 30,
                DayOfWeek.Monday, "6am", 55.6f, new[] { "Alias" },
                new[] { "Genre1", "Genre2", "Genre3", "Tag1", "Tag2" }, "Overview");

            var target = new MetadataResult<Series>
            {
                Item = new Series()
            };

            var aniDbSeriesMetadataMappings = new TvDbSeriesMetadataMappings(new PluginConfiguration { MaxGenres = 3 });

            aniDbSeriesMetadataMappings.SeriesMappings.Iter(m => m.Apply(source, target));

            target.Item.Name.Should().Be("SeriesName");
            target.Item.PremiereDate.Should().Be(new DateTime(2017, 1, 2, 3, 4, 5));
            target.Item.Overview.Should().Be("Overview");
            target.Item.Genres.Should().BeEquivalentTo("Genre1", "Genre2", "Genre3");
            target.Item.Tags.Should().BeEquivalentTo("Tag1", "Tag2");
            target.Item.CommunityRating.Should().Be(55.6f);
            target.Item.AirDays.Should().BeEquivalentTo(DayOfWeek.Monday);
            target.Item.AirTime.Should().Be("6am");
        }
    }
}