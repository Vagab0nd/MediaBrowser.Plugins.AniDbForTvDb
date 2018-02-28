using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Mapping;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class GroupMappingEpisodeMapperTests
    {
        private GroupMappingEpisodeMapper _mapper;
        private ITvDbClient _tvDbClient;
        private IAniDbClient _aniDbClient;

        private TvDbEpisodeData _tvDbEpisodeA;
        private TvDbEpisodeData _tvDbEpisodeB;
        private AniDbEpisodeData _aniDbEpisodeA;
        private AniDbEpisodeData _aniDbEpisodeB;

        [SetUp]
        public void Setup()
        {
            _tvDbClient = Substitute.For<ITvDbClient>();
            _aniDbClient = Substitute.For<IAniDbClient>();

            _tvDbEpisodeA = TestData.TvDbTestData.Episode(12, 48, 2);
            _tvDbEpisodeB = TestData.TvDbTestData.Episode(45, 7, 2);
            _aniDbEpisodeA = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "44",
                    RawType = 1
                }
            };
            _aniDbEpisodeB = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "23",
                    RawType = 1
                }
            };

            _tvDbClient.GetEpisodesAsync(123)
                .Returns(Option<List<TvDbEpisodeData>>.Some(new List<TvDbEpisodeData> { _tvDbEpisodeA, _tvDbEpisodeB }));
            _aniDbClient.GetSeriesAsync(77)
                .Returns(Option<AniDbSeriesData>.Some(new AniDbSeriesData
                {
                    Episodes = new[] { _aniDbEpisodeA, _aniDbEpisodeB }
                }));

            _mapper = new GroupMappingEpisodeMapper(_tvDbClient, _aniDbClient, new ConsoleLogManager());
        }

        [Test]
        public async Task MapAniDbEpisodeAsync_NoEpisodeMapping_MapsUsingOffset()
        {
            var groupMapping = new EpisodeGroupMapping(1, 2, 4, null, null, new List<EpisodeMapping>());

            var result = await _mapper.MapAniDbEpisodeAsync(44, groupMapping, 123).ToOption();

            result.IsSome.Should().BeTrue();
            result.ValueUnsafe().Should().Be(_tvDbEpisodeA);
        }

        [Test]
        public async Task MapAniDbEpisodeAsync_HasEpisodeMapping_MapsUsingEpisodeMapping()
        {
            var groupMapping = new EpisodeGroupMapping(1, 2, 4, null, null, new List<EpisodeMapping>
            {
                new EpisodeMapping(44, 7)
            });

            var result = await _mapper.MapAniDbEpisodeAsync(44, groupMapping, 123).ToOption();

            result.IsSome.Should().BeTrue();
            result.ValueUnsafe().Should().Be(_tvDbEpisodeB);
        }

        [Test]
        public async Task MapAniDbEpisodeAsync_NoEpisodeInTvDbData_ReturnsNone()
        {
            var groupMapping = new EpisodeGroupMapping(1, 2, 4, null, null, new List<EpisodeMapping>());

            var result = await _mapper.MapAniDbEpisodeAsync(1, groupMapping, 123).ToOption();

            result.IsSome.Should().BeFalse();
        }

        [Test]
        public async Task MapTvDbEpisodeAsync_NoEpisodeMapping_MapsUsingOffset()
        {
            var groupMapping = new EpisodeGroupMapping(1, 2, 4, null, null, new List<EpisodeMapping>());

            var result = await _mapper.MapTvDbEpisodeAsync(48, groupMapping, 77).ToOption();

            result.IsSome.Should().BeTrue();
            result.ValueUnsafe().Should().Be(_aniDbEpisodeA);
        }

        [Test]
        public async Task MapTvDbEpisodeAsync_HasEpisodeMapping_MapsUsingEpisodeMapping()
        {
            var groupMapping = new EpisodeGroupMapping(1, 2, 4, null, null, new List<EpisodeMapping>
            {
                new EpisodeMapping(23, 7)
            });

            var result = await _mapper.MapTvDbEpisodeAsync(7, groupMapping, 77).ToOption();

            result.IsSome.Should().BeTrue();
            result.ValueUnsafe().Should().Be(_aniDbEpisodeB);
        }

        [Test]
        public async Task MapTvDbEpisodeAsync_NoEpisodeInAniDbData_ReturnsNone()
        {
            var groupMapping = new EpisodeGroupMapping(1, 2, 4, null, null, new List<EpisodeMapping>());

            var result = await _mapper.MapTvDbEpisodeAsync(1, groupMapping, 77).ToOption();

            result.IsSome.Should().BeFalse();
        }
    }
}
