using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class AniDbSourceMappingConfigurationTests
    {
        [SetUp]
        public void Setup()
        {
            _titleSelector = Substitute.For<ITitleSelector>();

            _titleSelector.SelectTitle(Arg.Any<ItemTitleData[]>(), TitleType.Localized, "en")
                .Returns(new ItemTitleData { Title = "SelectedTitle" });
        }

        private ITitleSelector _titleSelector;

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

            var aniDbSourceMappingConfiguration =
                new AniDbSourceMappingConfiguration(Substitute.For<IAniDbParser>(), _titleSelector);

            aniDbSourceMappingConfiguration.GetEpisodeMappings()
                .Select(m => m.TargetPropertyName)
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

            var aniDbSourceMappingConfiguration =
                new AniDbSourceMappingConfiguration(Substitute.For<IAniDbParser>(), _titleSelector);

            aniDbSourceMappingConfiguration.GetEpisodeMappings().Iter(m => m.Apply(source, target));

            target.Item.Name.Should().Be("SelectedTitle");
            target.Item.PremiereDate.Should().Be(new DateTime(2017, 1, 2, 3, 4, 5));
            target.Item.RunTimeTicks.Should().Be(21000000000L);
            target.Item.Overview.Should().Be("Description");
            target.Item.CommunityRating.Should().Be(45);
        }

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

            var aniDbSourceMappingConfiguration =
                new AniDbSourceMappingConfiguration(aniDbParser, _titleSelector);

            aniDbSourceMappingConfiguration.GetSeasonMappings(1, true, TitleType.Localized, "en")
                .Select(m => m.TargetPropertyName)
                .ShouldAllBeEquivalentTo(expectedMappedFields);
        }

        [Test]
        public void SeasonMappings_MapsAllFields()
        {
            var source = new AniDbSeriesData
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
            };

            var aniDbParser = Substitute.For<IAniDbParser>();

            aniDbParser.FormatDescription("Description").Returns("FormattedDescription");
            aniDbParser.GetGenres(source, 1, true).Returns(new List<string> { "Genre" });
            aniDbParser.GetTags(source, 1, true).Returns(new List<string> { "Tags" });
            aniDbParser.GetStudios(source).Returns(new List<string> { "Studio" });

            var target = new MetadataResult<Season>
            {
                Item = new Season()
            };

            var aniDbSourceMappingConfiguration =
                new AniDbSourceMappingConfiguration(aniDbParser, _titleSelector);

            aniDbSourceMappingConfiguration.GetSeasonMappings(1, true, TitleType.Localized, "en")
                .Iter(m => m.Apply(source, target));

            target.Item.Name.Should().Be("SelectedTitle");
            target.Item.PremiereDate.Should().Be(new DateTime(2017, 1, 2, 3, 4, 5));
            target.Item.EndDate.Should().Be(new DateTime(2017, 5, 4, 3, 2, 1));
            target.Item.Overview.Should().Be("FormattedDescription");
            target.Item.Genres.Should().BeEquivalentTo("Genre");
            target.Item.Studios.Should().BeEquivalentTo("Studio");
            target.Item.Tags.Should().BeEquivalentTo("Tags");
            target.Item.CommunityRating.Should().Be(45);
        }

        [Test]
        public void SeriesMappings_HasMappingsForAllFields()
        {
            var aniDbParser = Substitute.For<IAniDbParser>();

            var expectedMappedFields = new[]
            {
                nameof(Series.PremiereDate),
                nameof(Series.EndDate),
                nameof(Series.Name),
                nameof(Series.Overview),
                nameof(Series.CommunityRating),
                nameof(Series.Studios),
                nameof(Series.Genres),
                nameof(Series.Tags),
                nameof(MetadataResult<Series>.People)
            };

            var aniDbSourceMappingConfiguration =
                new AniDbSourceMappingConfiguration(aniDbParser, _titleSelector);

            aniDbSourceMappingConfiguration.GetSeriesMappings(1, true, true, TitleType.Localized, "en")
                .Select(m => m.TargetPropertyName)
                .ShouldAllBeEquivalentTo(expectedMappedFields);
        }

        [Test]
        public void SeriesMappings_MapsAllFields()
        {
            var source = new AniDbSeriesData
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
            };

            var aniDbParser = Substitute.For<IAniDbParser>();

            aniDbParser.FormatDescription("Description").Returns("FormattedDescription");
            aniDbParser.GetGenres(source, 1, true).Returns(new List<string> { "Genre" });
            aniDbParser.GetTags(source, 1, true).Returns(new List<string> { "Tags" });
            aniDbParser.GetStudios(source).Returns(new List<string> { "Studio" });
            aniDbParser.GetPeople(source)
                .Returns(new List<PersonInfo>
                {
                    new PersonInfo
                    {
                        Name = "Person",
                        Role = "Role"
                    }
                });

            var target = new MetadataResult<Series>
            {
                Item = new Series()
            };

            var aniDbSourceMappingConfiguration =
                new AniDbSourceMappingConfiguration(aniDbParser, _titleSelector);

            aniDbSourceMappingConfiguration.GetSeriesMappings(1, true, true, TitleType.Localized, "en")
                .Iter(m => m.Apply(source, target));

            target.Item.Name.Should().Be("SelectedTitle");
            target.Item.PremiereDate.Should().Be(new DateTime(2017, 1, 2, 3, 4, 5));
            target.Item.EndDate.Should().Be(new DateTime(2017, 5, 4, 3, 2, 1));
            target.Item.Overview.Should().Be("FormattedDescription");
            target.Item.Genres.Should().BeEquivalentTo("Genre");
            target.Item.Studios.Should().BeEquivalentTo("Studio");
            target.Item.Tags.Should().BeEquivalentTo("Tags");
            target.Item.CommunityRating.Should().Be(45);

            target.People.ShouldBeEquivalentTo(new List<PersonInfo>
            {
                new PersonInfo
                {
                    Name = "Person",
                    Role = "Role"
                }
            });
        }
    }
}