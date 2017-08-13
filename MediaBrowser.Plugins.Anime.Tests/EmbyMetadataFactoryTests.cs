using System;
using System.Linq;
using FluentAssertions;
using Functional.Maybe;
using MediaBrowser.Plugins.Anime.AniDb.Data;
using MediaBrowser.Plugins.Anime.Configuration;
using MediaBrowser.Plugins.Anime.Providers.AniDb2;
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
        public void CreateSeasonMetadataResult_NoTags_DoesNotSetGenres()
        {
            var series = new AniDbSeries().WithoutTags();

            _titleSelector.SelectTitle(null, TitlePreferenceType.Localized, null)
                .ReturnsForAnyArgs(series.Titles.First().ToMaybe());

            var metadataFactory = new EmbyMetadataFactory(_titleSelector, _pluginConfiguration);

            var metadata = metadataFactory.CreateSeasonMetadataResult(series, 1, "en");

            metadata.Item.Genres.Should().BeNullOrEmpty();
        }

        [Test]
        public void CreateSeasonMetadataResult_NoDescription_DoesNotThrow()
        {
            var series = new AniDbSeries().WithStandardData();

            series.Description = null;

            _titleSelector.SelectTitle(null, TitlePreferenceType.Localized, null)
                .ReturnsForAnyArgs(series.Titles.First().ToMaybe());

            var metadataFactory = new EmbyMetadataFactory(_titleSelector, _pluginConfiguration);

            Action action = () => metadataFactory.CreateSeasonMetadataResult(series, 1, "en");

            action.ShouldNotThrow();
        }
    }
}