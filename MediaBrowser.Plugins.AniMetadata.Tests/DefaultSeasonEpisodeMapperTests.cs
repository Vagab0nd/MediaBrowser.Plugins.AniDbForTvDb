using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.Mapping;
using MediaBrowser.Plugins.AniMetadata.Tests.TestData;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class DefaultSeasonEpisodeMapperTests
    {
        [SetUp]
        public void Setup()
        {
            _logManager = new ConsoleLogManager();
            _tvDbClient = Substitute.For<ITvDbClient>();
            _seriesMapping = Substitute.For<ISeriesMapping>();
        }

        private ILogManager _logManager;
        private ITvDbClient _tvDbClient;
        private ISeriesMapping _seriesMapping;

        [Test]
        public async Task MapEpisodeAsync_AbsoluteDefaultTvDbSeason_MapsOnAbsoluteEpisodeIndex()
        {
            _seriesMapping.Ids.Returns(new SeriesIds(12, 33, Option<int>.None, Option<int>.None));
            _seriesMapping.DefaultTvDbSeason.Returns(new AbsoluteTvDbSeason());

            var tvDbEpisodeData = TvDbTestData.Episode(3);

            _tvDbClient.GetEpisodeAsync(33, 10).Returns(tvDbEpisodeData);

            var defaultSeasonEpisodeMapper = new DefaultSeasonEpisodeMapper(_tvDbClient, _logManager);

            var result = await defaultSeasonEpisodeMapper.MapEpisodeAsync(10, _seriesMapping);

            await _tvDbClient.Received(1).GetEpisodeAsync(33, 10);
            result.ValueUnsafe().Should().Be(tvDbEpisodeData);
        }

        [Test]
        public async Task MapEpisodeAsync_AbsoluteDefaultTvDbSeason_NoEpisodeData_ReturnsNone()
        {
            _seriesMapping.Ids.Returns(new SeriesIds(12, 33, Option<int>.None, Option<int>.None));
            _seriesMapping.DefaultTvDbSeason.Returns(new AbsoluteTvDbSeason());

            var defaultSeasonEpisodeMapper = new DefaultSeasonEpisodeMapper(_tvDbClient, _logManager);

            var result = await defaultSeasonEpisodeMapper.MapEpisodeAsync(10, _seriesMapping);

            result.IsNone.Should().BeTrue();
        }

        [Test]
        public async Task MapEpisodeAsync_DefaultTvDbSeason_MapsOnSeasonAndEpisodeIndex()
        {
            _seriesMapping.Ids.Returns(new SeriesIds(12, 33, Option<int>.None, Option<int>.None));
            _seriesMapping.DefaultTvDbSeason.Returns(new TvDbSeason(54));
            _seriesMapping.DefaultTvDbEpisodeIndexOffset.Returns(7);

            var tvDbEpisodeData = TvDbTestData.Episode(3);

            _tvDbClient.GetEpisodeAsync(33, 54, 17).Returns(tvDbEpisodeData);

            var defaultSeasonEpisodeMapper = new DefaultSeasonEpisodeMapper(_tvDbClient, _logManager);

            var result = await defaultSeasonEpisodeMapper.MapEpisodeAsync(10, _seriesMapping);

            await _tvDbClient.Received(1).GetEpisodeAsync(33, 54, 17);
            result.ValueUnsafe().Should().Be(tvDbEpisodeData);
        }

        [Test]
        public async Task MapEpisodeAsync_DefaultTvDbSeason_NoEpisodeData_ReturnsNone()
        {
            _seriesMapping.Ids.Returns(new SeriesIds(12, 33, Option<int>.None, Option<int>.None));
            _seriesMapping.DefaultTvDbSeason.Returns(new TvDbSeason(54));
            _seriesMapping.DefaultTvDbEpisodeIndexOffset.Returns(7);

            var defaultSeasonEpisodeMapper = new DefaultSeasonEpisodeMapper(_tvDbClient, _logManager);

            var result = await defaultSeasonEpisodeMapper.MapEpisodeAsync(10, _seriesMapping);

            result.IsNone.Should().BeTrue();
        }

        [Test]
        public async Task MapEpisodeAsync_NoTvDbSeriesId_ReturnsNone()
        {
            _seriesMapping.Ids.Returns(new SeriesIds(12, Option<int>.None, Option<int>.None, Option<int>.None));

            var defaultSeasonEpisodeMapper = new DefaultSeasonEpisodeMapper(_tvDbClient, _logManager);

            var result = await defaultSeasonEpisodeMapper.MapEpisodeAsync(10, _seriesMapping);

            result.IsNone.Should().BeTrue();
        }
    }
}