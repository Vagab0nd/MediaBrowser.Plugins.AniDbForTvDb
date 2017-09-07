using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class AniDbMapperTests
    {
        [SetUp]
        public void Setup()
        {
            _tvDbClient = Substitute.For<ITvDbClient>();
            _logManager = Substitute.For<ILogManager>();
        }

        private ILogManager _logManager;
        private ITvDbClient _tvDbClient;

        [Test]
        public void GetMappedSeriesIds_MatchingMapping_ReturnsMappedIds()
        {
            var mappingData = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(1, 142, 124, 556),
                    new TvDbSeason(35), 0, null, null)
            });

            var aniDbMapper = new AniDbMapper(mappingData, _tvDbClient, _logManager);

            var seriesIds = aniDbMapper.GetMappedSeriesIds(1);

            seriesIds.IsSome.Should().BeTrue();

            seriesIds.ValueUnsafe().TvDbSeriesId.ValueUnsafe().Should().Be(142);
            seriesIds.ValueUnsafe().ImdbSeriesId.ValueUnsafe().Should().Be(124);
            seriesIds.ValueUnsafe().TmDbSeriesId.ValueUnsafe().Should().Be(556);
        }

        [Test]
        public void GetMappedSeriesIds_NoMapping_ReturnsNone()
        {
            var aniDbMapper = new AniDbMapper(new MappingList(new List<SeriesMapping>()), _tvDbClient, _logManager);

            aniDbMapper.GetMappedSeriesIds(1).IsSome.Should().BeFalse();
        }

        [Test]
        public void GetMappedSeriesIds_SetImdbId_ReturnsImdbId()
        {
            var mappingData = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(1, Option<int>.None, 523, Option<int>.None),
                    new TvDbSeason(35), 0, null, null)
            });

            var aniDbMapper = new AniDbMapper(mappingData, _tvDbClient, _logManager);

            var seriesIds = aniDbMapper.GetMappedSeriesIds(1);

            seriesIds.ValueUnsafe().ImdbSeriesId.IsSome.Should().BeTrue();
            seriesIds.ValueUnsafe().ImdbSeriesId.ValueUnsafe().Should().Be(523);
        }

        [Test]
        public void GetMappedSeriesIds_SetTmdbId_ReturnsTvDbId()
        {
            var mappingData = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(1, Option<int>.None, Option<int>.None, 677),
                    new TvDbSeason(35), 0, null, null)
            });

            var aniDbMapper = new AniDbMapper(mappingData, _tvDbClient, _logManager);

            var seriesIds = aniDbMapper.GetMappedSeriesIds(1);

            seriesIds.ValueUnsafe().TmDbSeriesId.IsSome.Should().BeTrue();
            seriesIds.ValueUnsafe().TmDbSeriesId.ValueUnsafe().Should().Be(677);
        }

        [Test]
        public void GetMappedSeriesIds_SetTvDbId_ReturnsTvDbId()
        {
            var mappingData = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(1, 234, Option<int>.None, Option<int>.None),
                    new TvDbSeason(35), 0, null, null)
            });

            var aniDbMapper = new AniDbMapper(mappingData, _tvDbClient, _logManager);

            var seriesIds = aniDbMapper.GetMappedSeriesIds(1);

            seriesIds.ValueUnsafe().TvDbSeriesId.IsSome.Should().BeTrue();
            seriesIds.ValueUnsafe().TvDbSeriesId.ValueUnsafe().Should().Be(234);
        }

        [Test]
        public void GetMappedSeriesIds_UnsetImdbId_ReturnsNoImdbId()
        {
            var mappingData = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(1, 5, Option<int>.None, 5),
                    new TvDbSeason(35), 0, null, null)
            });

            var aniDbMapper = new AniDbMapper(mappingData, _tvDbClient, _logManager);

            var seriesIds = aniDbMapper.GetMappedSeriesIds(1);

            seriesIds.ValueUnsafe().ImdbSeriesId.IsSome.Should().BeFalse();
        }

        [Test]
        public void GetMappedSeriesIds_UnsetTmdbId_ReturnsNoTmdbId()
        {
            var mappingData = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(1, 5, 5, Option<int>.None),
                    new TvDbSeason(35), 0, null, null)
            });

            var aniDbMapper = new AniDbMapper(mappingData, _tvDbClient, _logManager);

            var seriesIds = aniDbMapper.GetMappedSeriesIds(1);

            seriesIds.ValueUnsafe().TmDbSeriesId.IsSome.Should().BeFalse();
        }

        [Test]
        public void GetMappedSeriesIds_UnsetTvDbId_ReturnsNoTvDbId()
        {
            var mappingData = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(1, Option<int>.None, 5, 5),
                    new TvDbSeason(35), 0, null, null)
            });

            var aniDbMapper = new AniDbMapper(mappingData, _tvDbClient, _logManager);

            var seriesIds = aniDbMapper.GetMappedSeriesIds(1);

            seriesIds.ValueUnsafe().TvDbSeriesId.IsSome.Should().BeFalse();
        }

        [Test]
        [TestCase("3", 3)]
        [TestCase("6", 6)]
        [TestCase("4", 4)]
        public async Task GetMappedTvDbEpisodeId_AbsoluteTvDbSeason_ReturnsAbsoluteEpisodeNumber(
            string rawEpisodeNumber, int expectedTvDbEpisodeIndex)
        {
            var mappingData = new MappingList(new[]
            {
                new SeriesMapping(
                    new SeriesIds(1, Option<int>.None, Option<int>.None, Option<int>.None),
                    new AbsoluteTvDbSeason(),
                    4,
                    null,
                    null)
            });

            var aniDbMapper = new AniDbMapper(mappingData, _tvDbClient, _logManager);

            var result =
                await aniDbMapper.GetMappedTvDbEpisodeIdAsync(1,
                    new EpisodeNumberData { RawNumber = rawEpisodeNumber });

            result.ResultType().Should().Be(typeof(AbsoluteEpisodeNumber));

            result.Switch(
                x => { },
                absoluteEpisodeNumber => absoluteEpisodeNumber.EpisodeIndex.Should().Be(expectedTvDbEpisodeIndex),
                x => { });
        }

        [Test]
        public async Task GetMappedTvDbEpisodeId_MappedSpecialEpisodeWithPosition_ReturnsMappedFollowingTvDbId()
        {
            var mappingData = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(1, Option<int>.None, Option<int>.None, Option<int>.None),
                    new TvDbSeason(35), 4, new[]
                    {
                        new EpisodeGroupMapping(0, 0, 0, null, null, new[]
                        {
                            new EpisodeMapping(3, 3)
                        })
                    }, new List<SpecialEpisodePosition>
                    {
                        new SpecialEpisodePosition(3, 66)
                    })
            });

            var aniDbMapper = new AniDbMapper(mappingData, _tvDbClient, _logManager);

            var result = await aniDbMapper.GetMappedTvDbEpisodeIdAsync(1,
                new EpisodeNumberData { RawNumber = "3", RawType = 2 /* special */ });

            result.ResultType().Should().Be(typeof(TvDbEpisodeNumber));

            result.Switch(tvDbEpisodeNumber =>
                {
                    tvDbEpisodeNumber.SeasonIndex.Should().Be(0);
                    tvDbEpisodeNumber.EpisodeIndex.Should().Be(3);

                    tvDbEpisodeNumber.FollowingTvDbEpisodeNumber.IsSome.Should().BeTrue();
                    tvDbEpisodeNumber.FollowingTvDbEpisodeNumber.ValueUnsafe().SeasonIndex.Should().Be(35);
                    tvDbEpisodeNumber.FollowingTvDbEpisodeNumber.ValueUnsafe().EpisodeIndex.Should().Be(70);
                },
                x => { },
                x => { });
        }

        [Test]
        [TestCase("3", 3, 9)]
        [TestCase("6", 3, 12)]
        [TestCase("4", 3, 10)]
        public async Task GetMappedTvDbEpisodeId_MatchingEpisodeGroupMapping_ReturnsMappedEpisodeNumber(
            string rawEpisodeNumber, int expectedTvDbSeasonIndex, int expectedTvDbEpisodeIndex)
        {
            var mappingData = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(1, Option<int>.None, Option<int>.None, Option<int>.None),
                    new TvDbSeason(35), 4, new[]
                    {
                        new EpisodeGroupMapping(1, 3, 6, 3, 6, null)
                    }, null)
            });

            var aniDbMapper = new AniDbMapper(mappingData, _tvDbClient, _logManager);

            var result =
                await aniDbMapper.GetMappedTvDbEpisodeIdAsync(1,
                    new EpisodeNumberData { RawNumber = rawEpisodeNumber });

            result.ResultType().Should().Be(typeof(TvDbEpisodeNumber));

            result.Switch(tvDbEpisodeNumber =>
                {
                    tvDbEpisodeNumber.EpisodeIndex.Should().Be(expectedTvDbEpisodeIndex);
                    tvDbEpisodeNumber.SeasonIndex.Should().Be(expectedTvDbSeasonIndex);
                },
                x => { },
                x => { });
        }

        [Test]
        [TestCase("3", 3, 9)]
        [TestCase("5", 3, 33)]
        [TestCase("12", 3, 17)]
        public async Task
            GetMappedTvDbEpisodeId_MatchingEpisodeGroupMappingWithEpisodeMapping_ReturnsMappedEpisodeNumber(
                string rawEpisodeNumber, int expectedTvDbSeasonIndex, int expectedTvDbEpisodeIndex)
        {
            var mappingData = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(1, Option<int>.None, Option<int>.None, Option<int>.None),
                    new TvDbSeason(35), 4, new[]
                    {
                        new EpisodeGroupMapping(1, 3, 6, 3, 6,
                            new[]
                            {
                                new EpisodeMapping(1, 5),
                                new EpisodeMapping(5, 33),
                                new EpisodeMapping(12, 17)
                            })
                    }, null)
            });

            var aniDbMapper = new AniDbMapper(mappingData, _tvDbClient, _logManager);

            var result =
                await aniDbMapper.GetMappedTvDbEpisodeIdAsync(1,
                    new EpisodeNumberData { RawNumber = rawEpisodeNumber });

            result.ResultType().Should().Be(typeof(TvDbEpisodeNumber));

            result.Switch(tvDbEpisodeNumber =>
                {
                    tvDbEpisodeNumber.EpisodeIndex.Should().Be(expectedTvDbEpisodeIndex);
                    tvDbEpisodeNumber.SeasonIndex.Should().Be(expectedTvDbSeasonIndex);
                },
                x => { },
                x => { });
        }

        [Test]
        public async Task GetMappedTvDbEpisodeId_NoEpisodeGroupMapping_ReturnsDefaultEpisodeNumber()
        {
            var mappingData = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(1, Option<int>.None, Option<int>.None, Option<int>.None),
                    new TvDbSeason(35), 4, null, null)
            });

            var aniDbMapper = new AniDbMapper(mappingData, _tvDbClient, _logManager);

            var result = await aniDbMapper.GetMappedTvDbEpisodeIdAsync(1, new EpisodeNumberData { RawNumber = "3" });

            result.ResultType().Should().Be(typeof(TvDbEpisodeNumber));

            result.Switch(tvDbEpisodeNumber =>
                {
                    tvDbEpisodeNumber.EpisodeIndex.Should().Be(7);
                    tvDbEpisodeNumber.SeasonIndex.Should().Be(35);
                },
                x => { },
                x => { });
        }

        [Test]
        public async Task GetMappedTvDbEpisodeId_NoMapping_ReturnsUnmappedEpisodeNumber()
        {
            var aniDbMapper = new AniDbMapper(new MappingList(null), _tvDbClient, _logManager);

            var episodeNumber = await aniDbMapper.GetMappedTvDbEpisodeIdAsync(1, new EpisodeNumberData());

            episodeNumber.ResultType().Should().Be(typeof(UnmappedEpisodeNumber));
        }

        [Test]
        public async Task GetMappedTvDbEpisodeId_NullEpisodeNumber_ReturnsUnmappedEpisodeNumber()
        {
            var mappingData = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(1, Option<int>.None, Option<int>.None, Option<int>.None),
                    new TvDbSeason(35), 4, null, null)
            });

            var aniDbMapper = new AniDbMapper(mappingData, _tvDbClient, _logManager);

            var episodeNumber = await aniDbMapper.GetMappedTvDbEpisodeIdAsync(1, null);

            episodeNumber.ResultType().Should().Be(typeof(UnmappedEpisodeNumber));
        }

        [Test]
        public async Task GetMappedTvDbEpisodeId_SpecialEpisodeWithPosition_ReturnsMappedFollowingTvDbId()
        {
            var mappingData = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(1, Option<int>.None, Option<int>.None, Option<int>.None),
                    new TvDbSeason(35), 4, null, new List<SpecialEpisodePosition>
                    {
                        new SpecialEpisodePosition(3, 66)
                    })
            });

            var aniDbMapper = new AniDbMapper(mappingData, _tvDbClient, _logManager);

            var result = await aniDbMapper.GetMappedTvDbEpisodeIdAsync(1,
                new EpisodeNumberData { RawNumber = "3", RawType = 2 /* special */ });

            result.ResultType().Should().Be(typeof(TvDbEpisodeNumber));

            result.Switch(tvDbEpisodeNumber =>
                {
                    tvDbEpisodeNumber.SeasonIndex.Should().Be(35);
                    tvDbEpisodeNumber.EpisodeIndex.Should().Be(7);

                    tvDbEpisodeNumber.FollowingTvDbEpisodeNumber.IsSome.Should().BeTrue();
                    tvDbEpisodeNumber.FollowingTvDbEpisodeNumber.ValueUnsafe().SeasonIndex.Should().Be(35);
                    tvDbEpisodeNumber.FollowingTvDbEpisodeNumber.ValueUnsafe().EpisodeIndex.Should().Be(70);
                },
                x => { },
                x => { });
        }
    }
}