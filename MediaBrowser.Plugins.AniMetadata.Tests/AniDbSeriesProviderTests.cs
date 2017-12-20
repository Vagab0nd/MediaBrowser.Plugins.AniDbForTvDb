using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.Mapping;
using MediaBrowser.Plugins.AniMetadata.Providers;
using MediaBrowser.Plugins.AniMetadata.Providers.AniDb;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class AniDbSeriesProviderTests
    {
        [SetUp]
        public void Setup()
        {
            _logManager = new ConsoleLogManager();
            _seriesMetadataFactory = Substitute.For<ISeriesMetadataFactory>();
            _seriesDataLoader = Substitute.For<ISeriesDataLoader>();
            _pluginConfiguration = Substitute.For<IPluginConfiguration>();

            _pluginConfiguration.ExcludedSeriesNames.Returns(new string[] { });

            _nullResult = new MetadataResult<Series>();

            _seriesMetadataFactory.NullResult.Returns(_nullResult);
        }

        private ILogManager _logManager;
        private ISeriesMetadataFactory _seriesMetadataFactory;
        private MetadataResult<Series> _nullResult;
        private ISeriesDataLoader _seriesDataLoader;
        private IPluginConfiguration _pluginConfiguration;

        [Test]
        public async Task GetMetadata_AniDbResult_SetsProviderIds()
        {
            var aniDbSeriesProvider =
                new AniDbSeriesProvider(_logManager, _seriesMetadataFactory, _seriesDataLoader, _pluginConfiguration);

            var seriesInfo = new SeriesInfo
            {
                Name = "AniDbTitle",
                MetadataLanguage = "en"
            };

            var seriesIds = new SeriesIds(1, Option<int>.None, 2, 4);

            var aniDbSeriesData = new AniDbSeriesData
            {
                Id = 4
            };

            _seriesDataLoader.GetSeriesDataAsync("AniDbTitle")
                .Returns(new AniDbOnlySeriesData(seriesIds, aniDbSeriesData));

            var expectedResult = new MetadataResult<Series>
            {
                Item = new Series()
            };

            _seriesMetadataFactory.CreateMetadata(aniDbSeriesData, "en").Returns(expectedResult);

            var result = await aniDbSeriesProvider.GetMetadata(seriesInfo, CancellationToken.None);

            result.Item.ProviderIds.Should().Contain(new KeyValuePair<string, string>(ProviderNames.AniDb, "1"));
            result.Item.ProviderIds.Should()
                .Contain(new KeyValuePair<string, string>(MetadataProviders.Imdb.ToString(), "2"));
            result.Item.ProviderIds.Should()
                .Contain(new KeyValuePair<string, string>(MetadataProviders.Tmdb.ToString(), "4"));
        }

        [Test]
        public async Task GetMetadata_CombinedResult_SetsProviderIds()
        {
            var aniDbSeriesProvider =
                new AniDbSeriesProvider(_logManager, _seriesMetadataFactory, _seriesDataLoader, _pluginConfiguration);

            var seriesInfo = new SeriesInfo
            {
                Name = "AniDbTitle",
                MetadataLanguage = "en"
            };

            var seriesIds = new SeriesIds(1, 33, 2, 4);

            var aniDbSeriesData = new AniDbSeriesData
            {
                Id = 4
            };

            var tvDbSeriesData = new TvDbSeriesData(33, "Name", new DateTime(2017, 1, 1, 1, 1, 1), "", 2,
                AirDay.Monday, "", 4f, new List<string>(), new List<string>(), "Overview");

            _seriesDataLoader.GetSeriesDataAsync("AniDbTitle")
                .Returns(new CombinedSeriesData(seriesIds, aniDbSeriesData, tvDbSeriesData));

            var expectedResult = new MetadataResult<Series>
            {
                Item = new Series()
            };

            _seriesMetadataFactory.CreateMetadata(aniDbSeriesData, tvDbSeriesData, "en").Returns(expectedResult);

            var result = await aniDbSeriesProvider.GetMetadata(seriesInfo, CancellationToken.None);

            result.Item.ProviderIds.Should().Contain(new KeyValuePair<string, string>(ProviderNames.AniDb, "1"));
            result.Item.ProviderIds.Should()
                .Contain(new KeyValuePair<string, string>(MetadataProviders.Tvdb.ToString(), "33"));
            result.Item.ProviderIds.Should()
                .Contain(new KeyValuePair<string, string>(MetadataProviders.Imdb.ToString(), "2"));
            result.Item.ProviderIds.Should()
                .Contain(new KeyValuePair<string, string>(MetadataProviders.Tmdb.ToString(), "4"));
        }

        [Test]
        public async Task GetMetadata_ExcludedSeries_RemovesExistingProviderId()
        {
            _pluginConfiguration.ExcludedSeriesNames.Returns(new[] { "Excluded" });

            var aniDbSeriesProvider =
                new AniDbSeriesProvider(_logManager, _seriesMetadataFactory, _seriesDataLoader, _pluginConfiguration);

            var seriesInfo = new SeriesInfo
            {
                Name = "Excluded",
                MetadataLanguage = "en",
                ProviderIds = { { ProviderNames.AniDb, "1232" } }
            };

            await aniDbSeriesProvider.GetMetadata(seriesInfo, CancellationToken.None);

            seriesInfo.ProviderIds.Should().NotContainKey(ProviderNames.AniDb);
        }

        [Test]
        [TestCase("Excluded", "Excluded", true)]
        [TestCase("eXCLUDED", "Excluded", true)]
        [TestCase("NotExcluded", "Excluded", false)]
        [TestCase("", "Excluded", false)]
        [TestCase(null, "Excluded", false)]
        public async Task GetMetadata_ExcludedSeries_ReturnsNullResult(string seriesName, string excludedSeriesName,
            bool shouldBeExcluded)
        {
            _pluginConfiguration.ExcludedSeriesNames.Returns(new[] { excludedSeriesName });

            var aniDbSeriesProvider =
                new AniDbSeriesProvider(_logManager, _seriesMetadataFactory, _seriesDataLoader, _pluginConfiguration);

            var seriesInfo = new SeriesInfo
            {
                Name = seriesName,
                MetadataLanguage = "en"
            };

            var aniDbSeriesData = new AniDbSeriesData();

            var nonExcludedResult = new MetadataResult<Series>
            {
                Item = new Series()
            };

            _seriesDataLoader.GetSeriesDataAsync(seriesName)
                .Returns(new AniDbOnlySeriesData(new SeriesIds(1, 0, 0, 0), aniDbSeriesData));

            _seriesMetadataFactory.CreateMetadata(aniDbSeriesData, "en").Returns(nonExcludedResult);

            var result = await aniDbSeriesProvider.GetMetadata(seriesInfo, CancellationToken.None);

            result.Should().Be(shouldBeExcluded ? _nullResult : nonExcludedResult);
        }

        [Test]
        public async Task GetMetadata_NonBlankResult_StopsOtherProviders()
        {
            var aniDbSeriesProvider =
                new AniDbSeriesProvider(_logManager, _seriesMetadataFactory, _seriesDataLoader, _pluginConfiguration);

            var seriesInfo = new SeriesInfo
            {
                Name = "AniDbTitle",
                MetadataLanguage = "en",
                IndexNumber = 1,
                ParentIndexNumber = 1,
                ProviderIds = { { "Key", "1" } }
            };

            var seriesIds = new SeriesIds(1, 33, 2, 4);

            var aniDbSeriesData = new AniDbSeriesData
            {
                Id = 4
            };

            var tvDbSeriesData = new TvDbSeriesData(33, "Name", new DateTime(2017, 1, 1, 1, 1, 1), "", 2,
                AirDay.Monday, "", 4f, new List<string>(), new List<string>(), "Overview");

            _seriesDataLoader.GetSeriesDataAsync("AniDbTitle")
                .Returns(new CombinedSeriesData(seriesIds, aniDbSeriesData, tvDbSeriesData));

            var expectedResult = new MetadataResult<Series>
            {
                Item = new Series(),
                HasMetadata = true
            };

            _seriesMetadataFactory.CreateMetadata(aniDbSeriesData, tvDbSeriesData, "en").Returns(expectedResult);

            await aniDbSeriesProvider.GetMetadata(seriesInfo, CancellationToken.None);

            seriesInfo.Name.Should().BeEmpty();
            seriesInfo.IndexNumber.Should().BeNull();
            seriesInfo.ParentIndexNumber.Should().BeNull();
            seriesInfo.ProviderIds.Should().BeEmpty();
        }

        [Test]
        public async Task GetMetadata_ReturnsCreatedMetadataResult()
        {
            var aniDbSeriesProvider =
                new AniDbSeriesProvider(_logManager, _seriesMetadataFactory, _seriesDataLoader, _pluginConfiguration);

            var seriesInfo = new SeriesInfo
            {
                Name = "AniDbTitle",
                MetadataLanguage = "en"
            };

            var aniDbSeriesData = new AniDbSeriesData();

            var expectedResult = new MetadataResult<Series>
            {
                Item = new Series()
            };

            _seriesDataLoader.GetSeriesDataAsync("AniDbTitle")
                .Returns(new AniDbOnlySeriesData(new SeriesIds(1, 0, 0, 0), aniDbSeriesData));

            _seriesMetadataFactory.CreateMetadata(aniDbSeriesData, "en").Returns(expectedResult);

            var result = await aniDbSeriesProvider.GetMetadata(seriesInfo, CancellationToken.None);

            result.Should().Be(expectedResult);
        }
    }
}