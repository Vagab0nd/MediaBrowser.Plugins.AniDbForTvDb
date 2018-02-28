using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Mapping;
using MediaBrowser.Plugins.AniMetadata.Tests.TestData;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class EpisodeMapperTests
    {
        [SetUp]
        public void Setup()
        {
            _defaultSeasonEpisodeMapper = Substitute.For<IDefaultSeasonEpisodeMapper>();
            _groupMappingEpisodeMapper = Substitute.For<IGroupMappingEpisodeMapper>();

            _seriesMapping = new SeriesMapping(new SeriesIds(123, Option<int>.None, Option<int>.None, Option<int>.None),
                null, 2, new List<EpisodeGroupMapping>(), new List<SpecialEpisodePosition>());
        }

        private IDefaultSeasonEpisodeMapper _defaultSeasonEpisodeMapper;
        private IGroupMappingEpisodeMapper _groupMappingEpisodeMapper;
        private SeriesMapping _seriesMapping;

        [Test]
        public async Task MapAniDbEpisodeAsync_GroupMappingWithoutTvDbSeriesId_ReturnsNone()
        {
            var episodeMapper = new EpisodeMapper(_defaultSeasonEpisodeMapper, _groupMappingEpisodeMapper);

            var episodeData = await episodeMapper.MapAniDbEpisodeAsync(3, _seriesMapping,
                new EpisodeGroupMapping(1, 1, 3, 1, 1, new List<EpisodeMapping>())).ToOption();

            episodeData.IsNone.Should().BeTrue();
        }

        [Test]
        public async Task MapAniDbEpisodeAsync_GroupMappingWithTvDbSeriesId_UsesGroupMappingEpisodeMapper()
        {
            _seriesMapping = new SeriesMapping(new SeriesIds(123, 523, Option<int>.None, Option<int>.None), null, 2,
                new List<EpisodeGroupMapping>(), new List<SpecialEpisodePosition>());

            var episodeGroupMapping = new EpisodeGroupMapping(1, 1, 3, 1, 1, new List<EpisodeMapping>());

            var expected = TvDbTestData.Episode(3);
            _groupMappingEpisodeMapper.MapAniDbEpisodeAsync(3, episodeGroupMapping, 523).Returns(expected);

            var episodeMapper = new EpisodeMapper(_defaultSeasonEpisodeMapper, _groupMappingEpisodeMapper);

            var episodeData = await episodeMapper.MapAniDbEpisodeAsync(3, _seriesMapping, episodeGroupMapping).ToOption();

            episodeData.ValueUnsafe().Should().Be(expected);
        }

        [Test]
        public async Task MapAniDbEpisodeAsync_NoGroupMapping_UsesDefaultSeasonMapper()
        {
            var expected = TvDbTestData.Episode(3);
            _defaultSeasonEpisodeMapper.MapEpisodeAsync(3, _seriesMapping).Returns(expected);

            var episodeMapper = new EpisodeMapper(_defaultSeasonEpisodeMapper, _groupMappingEpisodeMapper);

            var episodeData = await episodeMapper.MapAniDbEpisodeAsync(3, _seriesMapping, Option<EpisodeGroupMapping>.None).ToOption();

            episodeData.ValueUnsafe().Should().Be(expected);
        }

        [Test]
        public async Task MapTvDbEpisodeAsync_UsesGroupMappingEpisodeMapper()
        {
            _seriesMapping = new SeriesMapping(new SeriesIds(324, 523, Option<int>.None, Option<int>.None), null, 2,
                new List<EpisodeGroupMapping>(), new List<SpecialEpisodePosition>());

            var episodeGroupMapping = new EpisodeGroupMapping(1, 1, 3, 1, 1, new List<EpisodeMapping>());

            var expected = new AniDbEpisodeData();
            _groupMappingEpisodeMapper.MapTvDbEpisodeAsync(3, episodeGroupMapping, 324).Returns(expected);

            var episodeMapper = new EpisodeMapper(_defaultSeasonEpisodeMapper, _groupMappingEpisodeMapper);

            var episodeData = await episodeMapper.MapTvDbEpisodeAsync(3, _seriesMapping, episodeGroupMapping).ToOption();

            episodeData.ValueUnsafe().Should().Be(expected);
        }
    }
}