using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Providers.TvDb;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class TvDbSeriesDataLoaderTests
    {
        [SetUp]
        public void Setup()
        {
            _logManager = new ConsoleLogManager();
            _aniDbClient = Substitute.For<IAniDbClient>();
            _tvDbClient = Substitute.For<ITvDbClient>();
            _mapper = Substitute.For<IAniDbMapper>();
        }

        private ILogManager _logManager;
        private IAniDbClient _aniDbClient;
        private IAniDbMapper _mapper;
        private ITvDbClient _tvDbClient;

        [Test]
        public async Task GetSeriesDataAsync_NoAniDbSeriesData_ReturnsNone()
        {
            var tvDbSeriesProvider =
                new TvDbSeriesDataLoader(_logManager, _aniDbClient, _tvDbClient);

            var result = await tvDbSeriesProvider.GetSeriesDataAsync("TvDbTitle");

            result.IsT2.Should().BeTrue();
        }

        [Test]
        public async Task GetSeriesDataAsync_NoMapper_ReturnsNone()
        {
            var tvDbSeriesProvider =
                new TvDbSeriesDataLoader(_logManager, _aniDbClient, _tvDbClient);

            var tvDbSeriesData = new TvDbSeriesData(4, "TvDbTitle", new DateTime(2017, 1, 1, 1, 1, 1), "", 2,
                DayOfWeek.Monday, "", 4f, new List<string>(), new List<string>(), "Overview");

            _tvDbClient.FindSeriesAsync("TvDbTitle").Returns(tvDbSeriesData);

            var result = await tvDbSeriesProvider.GetSeriesDataAsync("TvDbTitle");

            result.IsT2.Should().BeTrue();
        }

        [Test]
        public async Task GetSeriesDataAsync_NoSeriesIds_ReturnsNone()
        {
            var tvDbSeriesProvider =
                new TvDbSeriesDataLoader(_logManager, _aniDbClient, _tvDbClient);

            var tvDbSeriesData = new TvDbSeriesData(4, "TvDbTitle", new DateTime(2017, 1, 1, 1, 1, 1), "", 2,
                DayOfWeek.Monday, "", 4f, new List<string>(), new List<string>(), "Overview");

            _tvDbClient.FindSeriesAsync("TvDbTitle").Returns(tvDbSeriesData);
            _aniDbClient.GetMapperAsync().Returns(Option<IAniDbMapper>.Some(_mapper));

            var result = await tvDbSeriesProvider.GetSeriesDataAsync("TvDbTitle");

            result.IsT2.Should().BeTrue();
        }

        [Test]
        public async Task GetSeriesDataAsync_NoTvDbSeriesId_ReturnsNone()
        {
            var tvDbSeriesProvider =
                new TvDbSeriesDataLoader(_logManager, _aniDbClient, _tvDbClient);

            var seriesIds = new SeriesIds(1, Option<int>.None, 2, 4);

            var tvDbSeriesData = new TvDbSeriesData(33, "TvDbTitle", new DateTime(2017, 1, 1, 1, 1, 1), "", 2,
                DayOfWeek.Monday, "", 4f, new List<string>(), new List<string>(), "Overview");

            _tvDbClient.FindSeriesAsync("TvDbTitle").Returns(tvDbSeriesData);
            _aniDbClient.GetMapperAsync().Returns(Option<IAniDbMapper>.Some(_mapper));
            _mapper.GetMappedSeriesIdsFromTvDb(33).Returns(seriesIds);

            var result = await tvDbSeriesProvider.GetSeriesDataAsync("TvDbTitle");

            result.IsT2.Should().BeTrue();
        }

        [Test]
        public async Task GetSeriesDataAsync_TvDbSeriesData_ReturnsCombinedResult()
        {
            var tvDbSeriesProvider =
                new TvDbSeriesDataLoader(_logManager, _aniDbClient, _tvDbClient);

            var seriesIds = new SeriesIds(1, 33, 2, 4);

            var tvDbSeriesData = new TvDbSeriesData(33, "TvDbTitle", new DateTime(2017, 1, 1, 1, 1, 1), "", 2,
                DayOfWeek.Monday, "", 4f, new List<string>(), new List<string>(), "Overview");

            var aniDbSeriesData = new AniDbSeriesData
            {
                Id = 1
            };

            _aniDbClient.GetSeriesAsync("1").Returns(aniDbSeriesData);

            _tvDbClient.FindSeriesAsync("TvDbTitle").Returns(tvDbSeriesData);
            _aniDbClient.GetMapperAsync().Returns(Option<IAniDbMapper>.Some(_mapper));
            _mapper.GetMappedSeriesIdsFromTvDb(33).Returns(seriesIds);

            var result = await tvDbSeriesProvider.GetSeriesDataAsync("TvDbTitle");

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