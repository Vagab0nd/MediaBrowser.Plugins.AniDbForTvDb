using System;
using System.Linq;
using FluentAssertions;
using Functional.Maybe;
using MediaBrowser.Plugins.Anime.AniDb.Mapping;
using MediaBrowser.Plugins.Anime.AniDb.Series.Data;
using MediaBrowser.Plugins.Anime.Configuration;
using MediaBrowser.Plugins.Anime.Providers.AniDb;
using MediaBrowser.Plugins.Anime.Tests.TestData;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.Anime.Tests
{
    [TestFixture]
    public class EmbyMetadataFactoryTests
    {
        [SetUp]
        public void Setup()
        {
            _pluginConfiguration = new PluginConfiguration();
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
                .ReturnsForAnyArgs((episode.Titles.First() as ItemTitleData).ToMaybe());

            var metadataFactory = new EmbyMetadataFactory(_titleSelector, _pluginConfiguration);

            var result = metadataFactory.CreateEpisodeMetadataResult(episode,
                new MappedEpisodeResult(new TvDbEpisodeNumber(Maybe<int>.Nothing, 1, 1,
                    new TvDbEpisodeNumber(Maybe<int>.Nothing, 2, 5, Maybe<TvDbEpisodeNumber>.Nothing).ToMaybe())),
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
                .ReturnsForAnyArgs((episode.Titles.First() as ItemTitleData).ToMaybe());

            var metadataFactory = new EmbyMetadataFactory(_titleSelector, _pluginConfiguration);

            var result = metadataFactory.CreateEpisodeMetadataResult(episode,
                new MappedEpisodeResult(new TvDbEpisodeNumber(Maybe<int>.Nothing, 1, 1,
                    Maybe<TvDbEpisodeNumber>.Nothing)), "en");

            result.Item.AirsBeforeSeasonNumber.Should().BeNull();
            result.Item.AirsBeforeEpisodeNumber.Should().BeNull();
        }

        [Test]
        public void CreateSeasonMetadataResult_NoDescription_DoesNotThrow()
        {
            var series = new AniDbSeriesData().WithStandardData();

            series.Description = null;

            _titleSelector.SelectTitle(null, TitleType.Localized, null)
                .ReturnsForAnyArgs(series.Titles.First().ToMaybe());

            var metadataFactory = new EmbyMetadataFactory(_titleSelector, _pluginConfiguration);

            Action action = () => metadataFactory.CreateSeasonMetadataResult(series, 1, "en");

            action.ShouldNotThrow();
        }

        [Test]
        public void CreateSeasonMetadataResult_NoTags_DoesNotSetGenres()
        {
            var series = new AniDbSeriesData().WithoutTags();

            _titleSelector.SelectTitle(null, TitleType.Localized, null)
                .ReturnsForAnyArgs(series.Titles.First().ToMaybe());

            var metadataFactory = new EmbyMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateSeasonMetadataResult(series, 1, "en");

            metadata.Item.Genres.Should().BeNullOrEmpty();
        }
    }
}