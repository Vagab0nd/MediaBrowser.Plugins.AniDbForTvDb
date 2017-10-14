using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
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
    public class AniDbSeriesDataLoaderTests
    {
        [SetUp]
        public void Setup()
        {
            _logManager = new ConsoleLogManager();
            _aniDbClient = Substitute.For<IAniDbClient>();
            _tvDbClient = Substitute.For<ITvDbClient>();
            _mapper = Substitute.For<IDataMapper>();
        }

        private ILogManager _logManager;
        private IAniDbClient _aniDbClient;
        private IDataMapper _mapper;
        private ITvDbClient _tvDbClient;

        [Test]
        public async Task GetSeriesDataAsync_NoAniDbSeriesData_ReturnsNullResult()
        {
            var aniDbSeriesProvider =
                new AniDbSeriesDataLoader(_logManager, _aniDbClient);

            var result = await aniDbSeriesProvider.GetSeriesDataAsync("AniDbTitle");

            result.IsT2.Should().BeTrue();
        }

        [Test]
        public async Task GetSeriesDataAsync_NoMapper_ReturnsAniDbResult()
        {
            var aniDbSeriesProvider =
                new AniDbSeriesDataLoader(_logManager, _aniDbClient);

            var aniDbSeriesData = new AniDbSeriesData();

            _aniDbClient.FindSeriesAsync("AniDbTitle").Returns(aniDbSeriesData);

            var result = await aniDbSeriesProvider.GetSeriesDataAsync("AniDbTitle");

            result.IsT0.Should().BeTrue();
            result.AsT0.AniDbSeriesData.Should().Be(aniDbSeriesData);
        }

        [Test]
        public async Task GetSeriesDataAsync_NoSeriesIds_ReturnsAniDbResult()
        {
            var aniDbSeriesProvider =
                new AniDbSeriesDataLoader(_logManager, _aniDbClient);

            var aniDbSeriesData = new AniDbSeriesData();

            _aniDbClient.FindSeriesAsync("AniDbTitle").Returns(aniDbSeriesData);
            _aniDbClient.GetMapperAsync().Returns(Option<IDataMapper>.Some(_mapper));

            _mapper.MapSeriesDataAsync(aniDbSeriesData)
                .Returns(new AniDbOnlySeriesData(
                    new SeriesIds(324, Option<int>.None, Option<int>.None, Option<int>.None), aniDbSeriesData));

            var result = await aniDbSeriesProvider.GetSeriesDataAsync("AniDbTitle");

            result.IsT0.Should().BeTrue();
            result.AsT0.AniDbSeriesData.Should().Be(aniDbSeriesData);
        }

        [Test]
        public async Task GetSeriesDataAsync_NoTvDbSeriesData_ReturnsAniDbResult()
        {
            var aniDbSeriesProvider =
                new AniDbSeriesDataLoader(_logManager, _aniDbClient);

            var seriesIds = new SeriesIds(1, 33, 2, 4);

            var aniDbSeriesData = new AniDbSeriesData
            {
                Id = 4
            };

            var seriesData = new AniDbOnlySeriesData(seriesIds, aniDbSeriesData);

            _aniDbClient.FindSeriesAsync("AniDbTitle").Returns(aniDbSeriesData);
            _aniDbClient.GetMapperAsync().Returns(Option<IDataMapper>.Some(_mapper));
            _mapper.MapSeriesDataAsync(aniDbSeriesData).Returns(seriesData);

            var result = await aniDbSeriesProvider.GetSeriesDataAsync("AniDbTitle");

            result.AsT0.AniDbSeriesData.Should().Be(aniDbSeriesData);
        }

        [Test]
        public async Task GetSeriesDataAsync_NoTvDbSeriesId_ReturnsAniDbResult()
        {
            var aniDbSeriesProvider =
                new AniDbSeriesDataLoader(_logManager, _aniDbClient);

            var seriesIds = new SeriesIds(1, Option<int>.None, 2, 4);

            var aniDbSeriesData = new AniDbSeriesData
            {
                Id = 4
            };

            var seriesData = new AniDbOnlySeriesData(seriesIds, aniDbSeriesData);

            _aniDbClient.FindSeriesAsync("AniDbTitle").Returns(aniDbSeriesData);
            _aniDbClient.GetMapperAsync().Returns(Option<IDataMapper>.Some(_mapper));
            _mapper.MapSeriesDataAsync(aniDbSeriesData).Returns(seriesData);

            var result = await aniDbSeriesProvider.GetSeriesDataAsync("AniDbTitle");

            result.AsT0.AniDbSeriesData.Should().Be(aniDbSeriesData);
        }

        [Test]
        public async Task GetSeriesDataAsync_TvDbSeriesData_ReturnsCombinedResult()
        {
            var aniDbSeriesProvider =
                new AniDbSeriesDataLoader(_logManager, _aniDbClient);

            var seriesIds = new SeriesIds(1, 33, 2, 4);

            var aniDbSeriesData = new AniDbSeriesData
            {
                Id = 4
            };

            var tvDbSeriesData = new TvDbSeriesData(33, "Name", new DateTime(2017, 1, 1, 1, 1, 1), "", 2,
                DayOfWeek.Monday, "", 4f, new List<string>(), new List<string>(), "Overview");

            var seriesData = new CombinedSeriesData(seriesIds, aniDbSeriesData, tvDbSeriesData);

            _tvDbClient.GetSeriesAsync(33).Returns(tvDbSeriesData);

            _aniDbClient.FindSeriesAsync("AniDbTitle").Returns(aniDbSeriesData);
            _aniDbClient.GetMapperAsync().Returns(Option<IDataMapper>.Some(_mapper));
            _mapper.MapSeriesDataAsync(aniDbSeriesData).Returns(seriesData);

            var result = await aniDbSeriesProvider.GetSeriesDataAsync("AniDbTitle");

            result.IsT1.Should().BeTrue();
            result.AsT1.AniDbSeriesData.Should().Be(aniDbSeriesData);
            result.AsT1.TvDbSeriesData.Should().Be(tvDbSeriesData);

            result.AsT1.SeriesIds.AniDbSeriesId.Should().Be(1);
            result.AsT1.SeriesIds.TvDbSeriesId.ValueUnsafe().Should().Be(33);
            result.AsT1.SeriesIds.ImdbSeriesId.ValueUnsafe().Should().Be(2);
            result.AsT1.SeriesIds.TmDbSeriesId.ValueUnsafe().Should().Be(4);
        }
    }
}