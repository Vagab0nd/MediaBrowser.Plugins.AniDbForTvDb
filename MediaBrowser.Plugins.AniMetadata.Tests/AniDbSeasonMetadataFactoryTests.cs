using System;
using FluentAssertions;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class AniDbSeasonMetadataFactoryTests
    {
        [SetUp]
        public void Setup()
        {
            _pluginConfiguration = new TestPluginConfiguration { AddAnimeGenre = false };
            _aniDbParser = Substitute.For<IAniDbParser>();
            _titleSelector = Substitute.For<ITitleSelector>();
        }

        private ITitleSelector _titleSelector;
        private PluginConfiguration _pluginConfiguration;
        private IAniDbParser _aniDbParser;

        [Test]
        public void CreateMetadata_HasTitle_ReturnsPopulatedSeason()
        {
            var series = new AniDbSeriesData
            {
                Titles = new ItemTitleData[0],
                Description = "Description",
                StartDate = new DateTime(1, 2, 3, 4, 5, 6),
                EndDate = new DateTime(6, 5, 4, 3, 2, 1),
                Ratings = new RatingData[] { new PermanentRatingData { Value = 55.24f } }
            };

            _titleSelector.SelectTitle(null, TitleType.Japanese, null)
                .ReturnsForAnyArgs(new ItemTitleData { Title = "Title" });
            _aniDbParser.FormatDescription("Description").Returns("FormattedDescription");
            _aniDbParser.GetStudios(series).Returns(new[] { "Studio1", "Studio2" });
            _aniDbParser.GetGenres(series, _pluginConfiguration.MaxGenres, _pluginConfiguration.AddAnimeGenre).Returns(new[] { "Genre1", "Genre2" });
            _aniDbParser.GetTags(series, _pluginConfiguration.MaxGenres, _pluginConfiguration.AddAnimeGenre).Returns(new[] { "Tag1", "Tag2" });

            var metadataFactory = new AniDbSeasonMetadataFactory(_titleSelector, _aniDbParser, _pluginConfiguration);

            var metadata = metadataFactory.CreateMetadata(series, 1, "en");

            metadata.HasMetadata.Should().BeTrue();

            metadata.Item.Name.Should().Be("Title");
            metadata.Item.Overview.Should().Be("FormattedDescription");
            metadata.Item.PremiereDate.Should().Be(new DateTime(1, 2, 3, 4, 5, 6));
            metadata.Item.EndDate.Should().Be(new DateTime(6, 5, 4, 3, 2, 1));
            metadata.Item.CommunityRating.Should().Be(55.24f);
            metadata.Item.IndexNumber.Should().Be(1);
            metadata.Item.Studios.ShouldBeEquivalentTo(new[] { "Studio1", "Studio2" });
            metadata.Item.Genres.ShouldBeEquivalentTo(new[] { "Genre1", "Genre2" });
            metadata.Item.Tags.ShouldBeEquivalentTo(new[] { "Tag1", "Tag2" });
        }

        [Test]
        public void CreateMetadata_NoTitle_ReturnsNullResult()
        {
            var series = new AniDbSeriesData
            {
                Titles = new ItemTitleData[0]
            };
            
            var metadataFactory = new AniDbSeasonMetadataFactory(_titleSelector, _aniDbParser, _pluginConfiguration);

            metadataFactory.CreateMetadata(series, 1, "en").ShouldBeEquivalentTo(metadataFactory.NullSeasonResult);
        }

        [Test]
        public void CreateMetadata_SelectsTitle()
        {
            var series = new AniDbSeriesData
            {
                Titles = new ItemTitleData[0]
            };

            _pluginConfiguration.TitlePreference = TitleType.Japanese;

            var metadataFactory = new AniDbSeasonMetadataFactory(_titleSelector, _aniDbParser, _pluginConfiguration);

            metadataFactory.CreateMetadata(series, 1, "en");

            _titleSelector.Received(1).SelectTitle(series.Titles, TitleType.Japanese, "en");
        }
    }
}