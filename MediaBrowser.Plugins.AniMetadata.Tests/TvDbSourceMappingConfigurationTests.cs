using System;
using System.Linq;
using FluentAssertions;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class TvDbSourceMappingConfigurationTests
    {
        [Test]
        public void EpisodeMappings_HasMappingsForAllFields()
        {
            var expectedMappedFields = new[]
            {
                nameof(Episode.PremiereDate),
                nameof(Episode.Overview),
                nameof(Episode.CommunityRating)
            };

            var tvDbSourceMappingConfiguration = new TvDbSourceMappingConfiguration();

            tvDbSourceMappingConfiguration.GetEpisodeMappings(TitleType.Localized, "en")
                .Select(m => m.TargetPropertyName)
                .ShouldAllBeEquivalentTo(expectedMappedFields);
        }

        [Test]
        public void EpisodeMappings_MapsAllFields()
        {
            var source = new TvDbEpisodeData(3, "EpisodeName", 5, 2, 6, 123, new DateTime(2017, 4, 3, 12, 0, 2),
                "Overview", 5.23f, 23);

            var target = new MetadataResult<Episode>
            {
                Item = new Episode()
            };

            var tvDbSourceMappingConfiguration = new TvDbSourceMappingConfiguration();

            tvDbSourceMappingConfiguration.GetEpisodeMappings(TitleType.Localized, "en").Iter(m => m.Apply(source, target));
            
            target.Item.PremiereDate.Should().Be(new DateTime(2017, 4, 3, 12, 0, 2));
            target.Item.Overview.Should().Be("Overview");
            target.Item.CommunityRating.Should().Be(5.23f);
        }

        [Test]
        public void SeasonMappings_HasMappingsForAllFields()
        {
            var expectedMappedFields = new[]
            {
                nameof(Season.Name)
            };

            var tvDbSourceMappingConfiguration = new TvDbSourceMappingConfiguration();

            tvDbSourceMappingConfiguration.GetSeasonMappings(3, true, TitleType.Localized, "en")
                .Select(m => m.TargetPropertyName)
                .ShouldAllBeEquivalentTo(expectedMappedFields);
        }

        [Test]
        public void SeasonMappings_MapsAllFields()
        {
            var source = new TvDbSeasonData(1);

            var target = new MetadataResult<Season>
            {
                Item = new Season()
            };

            var tvDbSourceMappingConfiguration = new TvDbSourceMappingConfiguration();

            tvDbSourceMappingConfiguration.GetSeasonMappings(3, true, TitleType.Localized, "en").Iter(m => m.Apply(source, target));

            target.Item.Name.Should().Be("Season 1");
        }

        [Test]
        public void SeriesMappings_HasMappingsForAllFields()
        {
            var expectedMappedFields = new[]
            {
                nameof(Series.PremiereDate),
                nameof(Series.Overview),
                nameof(Series.CommunityRating),
                nameof(Series.Genres),
                nameof(Series.Tags),
                nameof(Series.AirDays),
                nameof(Series.AirTime)
            };

            var tvDbSourceMappingConfiguration = new TvDbSourceMappingConfiguration();

            tvDbSourceMappingConfiguration.GetSeriesMappings(3, true, true, TitleType.Localized, "en")
                .Select(m => m.TargetPropertyName)
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

            var tvDbSourceMappingConfiguration = new TvDbSourceMappingConfiguration();

            tvDbSourceMappingConfiguration.GetSeriesMappings(3, true, true, TitleType.Localized, "en").Iter(m => m.Apply(source, target));
            
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