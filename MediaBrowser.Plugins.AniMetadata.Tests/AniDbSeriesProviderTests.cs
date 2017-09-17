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
using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Providers;
using MediaBrowser.Plugins.AniMetadata.Providers.AniDb;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
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
            _nullResult = new MetadataResult<Series>();

            _seriesMetadataFactory.NullSeriesResult.Returns(_nullResult);
        }

        private ILogManager _logManager;
        private ISeriesMetadataFactory _seriesMetadataFactory;
        private MetadataResult<Series> _nullResult;
        private ISeriesDataLoader _seriesDataLoader;

        [Test]
        public async Task GetMetadata_AniDbResult_SetsProviderIds()
        {
            var aniDbSeriesProvider =
                new AniDbSeriesProvider(_logManager, _seriesMetadataFactory, _seriesDataLoader);

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

            _seriesDataLoader.GetSeriesDataAsync(seriesInfo).Returns(new SeriesData(seriesIds, aniDbSeriesData));

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
                new AniDbSeriesProvider(_logManager, _seriesMetadataFactory, _seriesDataLoader);

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

            var tvDbSeriesData = new TvDbSeriesData(33, "Name", new List<string>(), new List<string>(), "Overview");

            _seriesDataLoader.GetSeriesDataAsync(seriesInfo)
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
        public async Task GetMetadata_ReturnsCreatedMetadataResult()
        {
            var aniDbSeriesProvider =
                new AniDbSeriesProvider(_logManager, _seriesMetadataFactory, _seriesDataLoader);

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

            _seriesDataLoader.GetSeriesDataAsync(seriesInfo)
                .Returns(new SeriesData(new SeriesIds(1, 0, 0, 0), aniDbSeriesData));

            _seriesMetadataFactory.CreateMetadata(aniDbSeriesData, "en").Returns(expectedResult);

            var result = await aniDbSeriesProvider.GetMetadata(seriesInfo, CancellationToken.None);

            result.Should().Be(expectedResult);
        }
    }
}