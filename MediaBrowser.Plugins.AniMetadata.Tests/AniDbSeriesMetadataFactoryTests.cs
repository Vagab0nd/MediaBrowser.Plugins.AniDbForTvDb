using System.Linq;
using FluentAssertions;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.Tests.TestData;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class AniDbSeriesMetadataFactoryTests
    {
        [SetUp]
        public void Setup()
        {
            _pluginConfiguration = new PluginConfiguration { AddAnimeGenre = false };

            _titleSelector = Substitute.For<ITitleSelector>();
        }

        private ITitleSelector _titleSelector;
        private PluginConfiguration _pluginConfiguration;

        [Test]
        public void CreateMetadata_AddAnimeGenreIsFalse_DoesNotAddAnimeGenre()
        {
            var series = new AniDbSeriesData().WithStandardData();
            series.Tags = new TagData[0];

            _titleSelector.SelectTitle(null, TitleType.Localized, null)
                .ReturnsForAnyArgs(series.Titles.First());

            _pluginConfiguration.MaxGenres = 2;
            _pluginConfiguration.AddAnimeGenre = false;

            var metadataFactory = new AniDbSeriesMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateMetadata(series, "en");

            metadata.Item.Genres.Should().BeEmpty();
        }

        [Test]
        public void CreateMetadata_AddAnimeGenreIsTrue_AddsAnimeGenre()
        {
            var series = new AniDbSeriesData().WithStandardData();
            series.Tags = new TagData[0];

            _titleSelector.SelectTitle(null, TitleType.Localized, null)
                .ReturnsForAnyArgs(series.Titles.First());

            _pluginConfiguration.MaxGenres = 2;
            _pluginConfiguration.AddAnimeGenre = true;

            var metadataFactory = new AniDbSeriesMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateMetadata(series, "en");

            metadata.Item.Genres.Should().BeEquivalentTo("Anime");
        }

        [Test]
        public void CreateMetadata_ConfiguredForExtraGenresToTags_AddsExcessGenresToTags()
        {
            var series = new AniDbSeriesData().WithStandardData();

            series.Tags = new[]
            {
                new TagData
                {
                    Id = 55,
                    Weight = 400,
                    Name = "Tag1"
                },
                new TagData
                {
                    Id = 46,
                    Weight = 500,
                    Name = "Tag2"
                }
            };

            _titleSelector.SelectTitle(null, TitleType.Localized, null)
                .ReturnsForAnyArgs(series.Titles.First());

            _pluginConfiguration.MaxGenres = 1;
            _pluginConfiguration.MoveExcessGenresToTags = true;

            var metadataFactory = new AniDbSeriesMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateMetadata(series, "en");

            metadata.Item.Tags.Should().BeEquivalentTo("Tag1");
        }

        [Test]
        public void CreateMetadata_ConfiguredForExtraGenresToTags_DoesNotChangeTags()
        {
            var series = new AniDbSeriesData().WithStandardData();

            series.Tags = new[]
            {
                new TagData
                {
                    Id = 55,
                    Weight = 400,
                    Name = "Tag1"
                },
                new TagData
                {
                    Id = 46,
                    Weight = 500,
                    Name = "Tag2"
                }
            };

            _titleSelector.SelectTitle(null, TitleType.Localized, null)
                .ReturnsForAnyArgs(series.Titles.First());

            _pluginConfiguration.MaxGenres = 1;
            _pluginConfiguration.MoveExcessGenresToTags = false;

            var metadataFactory = new AniDbSeriesMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateMetadata(series, "en");

            metadata.Item.Tags.Should().BeEmpty();
        }

        [Test]
        public void CreateMetadata_HasTags_SetsGenres()
        {
            var series = new AniDbSeriesData().WithStandardData();

            series.Tags = new[]
            {
                new TagData
                {
                    Id = 55,
                    Weight = 400,
                    Name = "Tag1"
                },
                new TagData
                {
                    Id = 46,
                    Weight = 400,
                    Name = "Tag2"
                }
            };

            _titleSelector.SelectTitle(null, TitleType.Localized, null)
                .ReturnsForAnyArgs(series.Titles.First());

            var metadataFactory = new AniDbSeriesMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateMetadata(series, "en");

            metadata.Item.Genres.Should().BeEquivalentTo("Tag1", "Tag2");
        }

        [Test]
        [TestCase(6)]
        [TestCase(22)]
        [TestCase(23)]
        [TestCase(60)]
        [TestCase(128)]
        [TestCase(129)]
        [TestCase(185)]
        [TestCase(216)]
        [TestCase(242)]
        [TestCase(255)]
        [TestCase(268)]
        [TestCase(269)]
        [TestCase(289)]
        public void CreateMetadata_IgnoresSpecificTags(int id)
        {
            var series = new AniDbSeriesData().WithStandardData();

            series.Tags = new[]
            {
                new TagData
                {
                    Id = id,
                    Weight = 600,
                    Name = "Tag1"
                }
            };

            _titleSelector.SelectTitle(null, TitleType.Localized, null)
                .ReturnsForAnyArgs(series.Titles.First());

            _pluginConfiguration.MaxGenres = 1;

            var metadataFactory = new AniDbSeriesMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateMetadata(series, "en");

            metadata.Item.Genres.Should().BeEmpty();
        }

        [Test]
        public void CreateMetadata_MoreTagsThanMaxGenres_TakesHighestWeighted()
        {
            var series = new AniDbSeriesData().WithStandardData();

            series.Tags = new[]
            {
                new TagData
                {
                    Id = 55,
                    Weight = 400,
                    Name = "Tag1"
                },
                new TagData
                {
                    Id = 46,
                    Weight = 500,
                    Name = "Tag2"
                }
            };

            _titleSelector.SelectTitle(null, TitleType.Localized, null)
                .ReturnsForAnyArgs(series.Titles.First());

            _pluginConfiguration.MaxGenres = 1;

            var metadataFactory = new AniDbSeriesMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateMetadata(series, "en");

            metadata.Item.Genres.Should().BeEquivalentTo("Tag2");
        }

        [Test]
        public void CreateMetadata_NoTags_DoesNotSetGenres()
        {
            var series = new AniDbSeriesData().WithoutTags();

            _titleSelector.SelectTitle(null, TitleType.Localized, null)
                .ReturnsForAnyArgs(series.Titles.First());

            var metadataFactory = new AniDbSeriesMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateMetadata(series, "en");

            metadata.Item.Genres.Should().BeNullOrEmpty();
        }

        [Test]
        public void CreateMetadata_TagWeightUnder400_IgnoresTags()
        {
            var series = new AniDbSeriesData().WithStandardData();

            series.Tags = new[]
            {
                new TagData
                {
                    Id = 55,
                    Weight = 100,
                    Name = "Tag1"
                },
                new TagData
                {
                    Id = 46,
                    Weight = 399,
                    Name = "Tag2"
                }
            };

            _titleSelector.SelectTitle(null, TitleType.Localized, null)
                .ReturnsForAnyArgs(series.Titles.First());

            _pluginConfiguration.MaxGenres = 2;
            _pluginConfiguration.MoveExcessGenresToTags = false;

            var metadataFactory = new AniDbSeriesMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateMetadata(series, "en");

            metadata.Item.Genres.Should().BeEmpty();
        }

        [Test]
        public void CreateMetadata_TooManyTags_AddsAnimeGenreFirst()
        {
            var series = new AniDbSeriesData().WithStandardData();

            series.Tags = new[]
            {
                new TagData
                {
                    Id = 55,
                    Weight = 500,
                    Name = "Tag1"
                },
                new TagData
                {
                    Id = 46,
                    Weight = 400,
                    Name = "Tag2"
                }
            };

            _titleSelector.SelectTitle(null, TitleType.Localized, null)
                .ReturnsForAnyArgs(series.Titles.First());

            _pluginConfiguration.MaxGenres = 2;
            _pluginConfiguration.AddAnimeGenre = true;

            var metadataFactory = new AniDbSeriesMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateMetadata(series, "en");

            metadata.Item.Genres.Should().BeEquivalentTo("Anime", "Tag1");
        }
    }
}