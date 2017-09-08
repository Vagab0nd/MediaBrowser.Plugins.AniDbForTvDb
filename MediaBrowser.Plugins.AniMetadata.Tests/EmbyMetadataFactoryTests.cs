using System;
using System.Linq;
using FluentAssertions;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
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
    }
}