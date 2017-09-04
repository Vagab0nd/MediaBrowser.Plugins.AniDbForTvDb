using System;
using System.Linq;
using FluentAssertions;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping;
using MediaBrowser.Plugins.AniMetadata.AniDb.Series.Data;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.Providers.AniDb;
using MediaBrowser.Plugins.AniMetadata.Tests.TestData;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class EmbyMetadataFactoryTests
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
        public void CreateEpisodeMetadataResult_FollowingEpisode_SetAirsBeforeFields()
        {
            var episode = new EpisodeData
            {
                Titles = new[]
                {
                    new EpisodeTitleData
                    {
                        Title = "EpisodeTitle"
                    }
                }
            };

            _titleSelector.SelectTitle(null, TitleType.Localized, null)
                .ReturnsForAnyArgs(episode.Titles.First());

            var metadataFactory = new EmbyMetadataFactory(_titleSelector, _pluginConfiguration);

            var result = metadataFactory.CreateEpisodeMetadataResult(episode,
                new TvDbEpisodeNumber(Option<int>.None, 1, 1,
                    new TvDbEpisodeNumber(Option<int>.None, 2, 5, Option<TvDbEpisodeNumber>.None)),
                "en");

            result.Item.AirsBeforeSeasonNumber.Should().Be(2);
            result.Item.AirsBeforeEpisodeNumber.Should().Be(5);
        }

        [Test]
        public void CreateEpisodeMetadataResult_NoFollowingEpisode_DoesNotSetAirsBeforeFields()
        {
            var episode = new EpisodeData
            {
                Titles = new[]
                {
                    new EpisodeTitleData
                    {
                        Title = "EpisodeTitle"
                    }
                }
            };

            _titleSelector.SelectTitle(null, TitleType.Localized, null)
                .ReturnsForAnyArgs(episode.Titles.First());

            var metadataFactory = new EmbyMetadataFactory(_titleSelector, _pluginConfiguration);

            var result = metadataFactory.CreateEpisodeMetadataResult(episode,
                new TvDbEpisodeNumber(Option<int>.None, 1, 1,
                    Option<TvDbEpisodeNumber>.None), "en");

            result.Item.AirsBeforeSeasonNumber.Should().BeNull();
            result.Item.AirsBeforeEpisodeNumber.Should().BeNull();
        }

        [Test]
        public void CreateSeasonMetadataResult_AddAnimeGenreIsFalse_DoesNotAddAnimeGenre()
        {
            var series = new AniDbSeriesData().WithStandardData();
            series.Tags = new TagData[0];

            _titleSelector.SelectTitle(null, TitleType.Localized, null)
                .ReturnsForAnyArgs(series.Titles.First());

            _pluginConfiguration.MaxGenres = 2;
            _pluginConfiguration.AddAnimeGenre = false;

            var metadataFactory = new EmbyMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateSeasonMetadataResult(series, 1, "en");

            metadata.Item.Genres.Should().BeEmpty();
        }

        [Test]
        public void CreateSeasonMetadataResult_AddAnimeGenreIsTrue_AddsAnimeGenre()
        {
            var series = new AniDbSeriesData().WithStandardData();
            series.Tags = new TagData[0];

            _titleSelector.SelectTitle(null, TitleType.Localized, null)
                .ReturnsForAnyArgs(series.Titles.First());

            _pluginConfiguration.MaxGenres = 2;
            _pluginConfiguration.AddAnimeGenre = true;

            var metadataFactory = new EmbyMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateSeasonMetadataResult(series, 1, "en");

            metadata.Item.Genres.Should().BeEquivalentTo("Anime");
        }

        [Test]
        public void CreateSeasonMetadataResult_ConfiguredForExtraGenresToTags_AddsExcessGenresToTags()
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

            var metadataFactory = new EmbyMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateSeasonMetadataResult(series, 1, "en");

            metadata.Item.Tags.Should().BeEquivalentTo("Tag1");
        }

        [Test]
        public void CreateSeasonMetadataResult_ConfiguredForExtraGenresToTags_DoesNotChangeTags()
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

            var metadataFactory = new EmbyMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateSeasonMetadataResult(series, 1, "en");

            metadata.Item.Tags.Should().BeEmpty();
        }

        [Test]
        public void CreateSeasonMetadataResult_HasTags_SetsGenres()
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

            var metadataFactory = new EmbyMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateSeasonMetadataResult(series, 1, "en");

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
        public void CreateSeasonMetadataResult_IgnoresSpecificTags(int id)
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

            var metadataFactory = new EmbyMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateSeasonMetadataResult(series, 1, "en");

            metadata.Item.Genres.Should().BeEmpty();
        }

        [Test]
        public void CreateSeasonMetadataResult_MoreTagsThanMaxGenres_TakesHighestWeighted()
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

            var metadataFactory = new EmbyMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateSeasonMetadataResult(series, 1, "en");

            metadata.Item.Genres.Should().BeEquivalentTo("Tag2");
        }

        [Test]
        public void CreateSeasonMetadataResult_NoDescription_DoesNotThrow()
        {
            var series = new AniDbSeriesData().WithStandardData();

            series.Description = null;

            _titleSelector.SelectTitle(null, TitleType.Localized, null)
                .ReturnsForAnyArgs(series.Titles.First());

            var metadataFactory = new EmbyMetadataFactory(_titleSelector, _pluginConfiguration);

            Action action = () => metadataFactory.CreateSeasonMetadataResult(series, 1, "en");

            action.ShouldNotThrow();
        }

        [Test]
        public void CreateSeasonMetadataResult_NoTags_DoesNotSetGenres()
        {
            var series = new AniDbSeriesData().WithoutTags();

            _titleSelector.SelectTitle(null, TitleType.Localized, null)
                .ReturnsForAnyArgs(series.Titles.First());

            var metadataFactory = new EmbyMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateSeasonMetadataResult(series, 1, "en");

            metadata.Item.Genres.Should().BeNullOrEmpty();
        }

        [Test]
        public void CreateSeasonMetadataResult_TagWeightUnder400_IgnoresTags()
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

            var metadataFactory = new EmbyMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateSeasonMetadataResult(series, 1, "en");

            metadata.Item.Genres.Should().BeEmpty();
        }

        [Test]
        public void CreateSeasonMetadataResult_TooManyTags_AddsAnimeGenreFirst()
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

            var metadataFactory = new EmbyMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateSeasonMetadataResult(series, 1, "en");

            metadata.Item.Genres.Should().BeEquivalentTo("Anime", "Tag1");
        }

        [Test]
        public void CreateSeriesMetadataResult_AddAnimeGenreIsFalse_DoesNotAddAnimeGenre()
        {
            var series = new AniDbSeriesData().WithStandardData();
            series.Tags = new TagData[0];

            _titleSelector.SelectTitle(null, TitleType.Localized, null)
                .ReturnsForAnyArgs(series.Titles.First());

            _pluginConfiguration.MaxGenres = 2;
            _pluginConfiguration.AddAnimeGenre = false;

            var metadataFactory = new EmbyMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateSeriesMetadataResult(series, "en");

            metadata.Item.Genres.Should().BeEmpty();
        }

        [Test]
        public void CreateSeriesMetadataResult_AddAnimeGenreIsTrue_AddsAnimeGenre()
        {
            var series = new AniDbSeriesData().WithStandardData();
            series.Tags = new TagData[0];

            _titleSelector.SelectTitle(null, TitleType.Localized, null)
                .ReturnsForAnyArgs(series.Titles.First());

            _pluginConfiguration.MaxGenres = 2;
            _pluginConfiguration.AddAnimeGenre = true;

            var metadataFactory = new EmbyMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateSeriesMetadataResult(series, "en");

            metadata.Item.Genres.Should().BeEquivalentTo("Anime");
        }

        [Test]
        public void CreateSeriesMetadataResult_ConfiguredForExtraGenresToTags_AddsExcessGenresToTags()
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

            var metadataFactory = new EmbyMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateSeriesMetadataResult(series, "en");

            metadata.Item.Tags.Should().BeEquivalentTo("Tag1");
        }

        [Test]
        public void CreateSeriesMetadataResult_ConfiguredForExtraGenresToTags_DoesNotChangeTags()
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

            var metadataFactory = new EmbyMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateSeriesMetadataResult(series, "en");

            metadata.Item.Tags.Should().BeEmpty();
        }

        [Test]
        public void CreateSeriesMetadataResult_HasTags_SetsGenres()
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

            var metadataFactory = new EmbyMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateSeriesMetadataResult(series, "en");

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
        public void CreateSeriesMetadataResult_IgnoresSpecificTags(int id)
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

            var metadataFactory = new EmbyMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateSeasonMetadataResult(series, 1, "en");

            metadata.Item.Genres.Should().BeEmpty();
        }

        [Test]
        public void CreateSeriesMetadataResult_MoreTagsThanMaxGenres_TakesHighestWeighted()
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

            var metadataFactory = new EmbyMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateSeriesMetadataResult(series, "en");

            metadata.Item.Genres.Should().BeEquivalentTo("Tag2");
        }

        [Test]
        public void CreateSeriesMetadataResult_NoTags_DoesNotSetGenres()
        {
            var series = new AniDbSeriesData().WithoutTags();

            _titleSelector.SelectTitle(null, TitleType.Localized, null)
                .ReturnsForAnyArgs(series.Titles.First());

            var metadataFactory = new EmbyMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateSeriesMetadataResult(series, "en");

            metadata.Item.Genres.Should().BeNullOrEmpty();
        }

        [Test]
        public void CreateSeriesMetadataResult_TagWeightUnder400_IgnoresTags()
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

            var metadataFactory = new EmbyMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateSeriesMetadataResult(series, "en");

            metadata.Item.Genres.Should().BeEmpty();
        }

        [Test]
        public void CreateSeriesMetadataResult_TooManyTags_AddsAnimeGenreFirst()
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

            var metadataFactory = new EmbyMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateSeriesMetadataResult(series, "en");

            metadata.Item.Genres.Should().BeEquivalentTo("Anime", "Tag1");
        }
    }
}