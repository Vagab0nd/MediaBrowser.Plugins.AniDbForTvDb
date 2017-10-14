using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Mapping;
using MediaBrowser.Plugins.AniMetadata.Providers.AniDb;
using MediaBrowser.Plugins.AniMetadata.Tests.TestData;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class DataMapperTests
    {
        [SetUp]
        public void Setup()
        {
            _tvDbClient = Substitute.For<ITvDbClient>();
            _logManager = new ConsoleLogManager();
            _episodeMatcher = Substitute.For<IEpisodeMatcher>();
            _mappingList = Substitute.For<IMappingList>();
            _episodeMapper = Substitute.For<IEpisodeMapper>();
        }

        private ILogManager _logManager;
        private ITvDbClient _tvDbClient;
        private IEpisodeMatcher _episodeMatcher;
        private IMappingList _mappingList;
        private IEpisodeMapper _episodeMapper;

        [Test]
        public async Task MapSeriesDataAsync_MatchingMapping_ReturnsMappedIds()
        {
            var seriesIds = new SeriesIds(324, 142, Option<int>.None, Option<int>.None);

            var seriesMapping = Substitute.For<ISeriesMapping>();
            seriesMapping.Ids.Returns(seriesIds);

            _mappingList.GetSeriesMappingFromAniDb(324).Returns(Option<ISeriesMapping>.Some(seriesMapping));

            var aniDbSeriesData = new AniDbSeriesData().WithStandardData();
            var tvDbSeriesData = TvDbTestData.Series(142);

            _tvDbClient.GetSeriesAsync(142).Returns(tvDbSeriesData);

            var dataMapper = new DataMapper(_mappingList, _tvDbClient, _episodeMatcher, _episodeMapper, _logManager);

            var seriesData = await dataMapper.MapSeriesDataAsync(aniDbSeriesData);

            seriesData.IsT1.Should().BeTrue();
            seriesData.AsT1.TvDbSeriesData.Should().Be(tvDbSeriesData);
            seriesData.AsT1.AniDbSeriesData.Should().Be(aniDbSeriesData);
            seriesData.AsT1.SeriesIds.Should().Be(seriesIds);
        }

        [Test]
        public async Task MapSeriesDataAsync_NoMapping_ReturnsOnlyAniDbSeriesData()
        {
            var aniDbSeriesData = new AniDbSeriesData().WithStandardData();

            var dataMapper = new DataMapper(_mappingList, _tvDbClient, _episodeMatcher, _episodeMapper, _logManager);

            var result = await dataMapper.MapSeriesDataAsync(aniDbSeriesData);

            result.IsT0.Should().BeTrue();
            result.AsT0.AniDbSeriesData.Should().Be(aniDbSeriesData);
        }

        [Test]
        public async Task MapEpisodeDataAsync_NoSeriesMapping_ReturnsAniDbEpisodeDataOnly()
        {
            var dataMapper = new DataMapper(_mappingList, _tvDbClient, _episodeMatcher, _episodeMapper, _logManager);

            var episodeData = await dataMapper.MapEpisodeDataAsync(new AniDbSeriesData(), new AniDbEpisodeData());

            episodeData.IsT0.Should().BeTrue();
        }

        [Test]
        public async Task MapEpisodeDataAsync_HasSeriesMapping_GetsEpisodeGroupMapping()
        {
            var seriesMapping = Substitute.For<ISeriesMapping>();
            seriesMapping.Ids.Returns(new SeriesIds(324, 142, Option<int>.None, Option<int>.None));

            _mappingList.GetSeriesMappingFromAniDb(324).Returns(Option<ISeriesMapping>.Some(seriesMapping));

            var aniDbSeriesData = new AniDbSeriesData().WithStandardData();
            var tvDbSeriesData = TvDbTestData.Series(142);

            var aniDbEpisodeData = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "33",
                    RawType = 1
                }
            };

            _tvDbClient.GetSeriesAsync(142).Returns(tvDbSeriesData);

            var dataMapper = new DataMapper(_mappingList, _tvDbClient, _episodeMatcher, _episodeMapper, _logManager);

            await dataMapper.MapEpisodeDataAsync(aniDbSeriesData, aniDbEpisodeData);

            seriesMapping.Received(1).GetEpisodeGroupMapping(aniDbEpisodeData.EpisodeNumber);
        }

        [Test]
        public async Task MapEpisodeDataAsync_HasSeriesMapping_GetsFollowingTvDbEpisode()
        {
            var seriesMapping = Substitute.For<ISeriesMapping>();
            seriesMapping.Ids.Returns(new SeriesIds(324, 142, Option<int>.None, Option<int>.None));

            _mappingList.GetSeriesMappingFromAniDb(324).Returns(Option<ISeriesMapping>.Some(seriesMapping));

            var aniDbSeriesData = new AniDbSeriesData().WithStandardData();
            var tvDbSeriesData = TvDbTestData.Series(142);

            var aniDbEpisodeData = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "33",
                    RawType = 1
                }
            };

            _tvDbClient.GetSeriesAsync(142).Returns(tvDbSeriesData);

            var dataMapper = new DataMapper(_mappingList, _tvDbClient, _episodeMatcher, _episodeMapper, _logManager);

            await dataMapper.MapEpisodeDataAsync(aniDbSeriesData, aniDbEpisodeData);

            seriesMapping.Received(1).GetSpecialEpisodePosition(aniDbEpisodeData.EpisodeNumber);
        }

        [Test]
        public async Task MapEpisodeDataAsync_HasMappedEpisodeData_ReturnsMappedEpisodeData()
        {
            var seriesMapping = Substitute.For<ISeriesMapping>();
            seriesMapping.Ids.Returns(new SeriesIds(324, 142, Option<int>.None, Option<int>.None));

            _mappingList.GetSeriesMappingFromAniDb(324).Returns(Option<ISeriesMapping>.Some(seriesMapping));

            var aniDbSeriesData = new AniDbSeriesData().WithStandardData();
            var tvDbSeriesData = TvDbTestData.Series(142);

            var aniDbEpisodeData = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "33",
                    RawType = 1
                }
            };
            var tvDbEpisodeData = TvDbTestData.Episode(3);

            _tvDbClient.GetSeriesAsync(142).Returns(tvDbSeriesData);

            _episodeMapper.MapEpisodeAsync(33, seriesMapping, Option<EpisodeGroupMapping>.None).Returns(tvDbEpisodeData);

            var dataMapper = new DataMapper(_mappingList, _tvDbClient, _episodeMatcher, _episodeMapper, _logManager);

            var episodeData = await dataMapper.MapEpisodeDataAsync(aniDbSeriesData, aniDbEpisodeData);

            episodeData.IsT1.Should().BeTrue();
            episodeData.AsT1.AniDbEpisodeData.Should().Be(aniDbEpisodeData);
            episodeData.AsT1.TvDbEpisodeData.Should().Be(tvDbEpisodeData);
            episodeData.AsT1.FollowingTvDbEpisodeData.IsT2.Should().BeTrue();
        }

        [Test]
        public async Task MapEpisodeDataAsync_NoMappedEpisodeData_ReturnsAniDbEpisodeDataOnly()
        {
            var seriesMapping = Substitute.For<ISeriesMapping>();
            seriesMapping.Ids.Returns(new SeriesIds(324, 142, Option<int>.None, Option<int>.None));

            _mappingList.GetSeriesMappingFromAniDb(142).Returns(Option<ISeriesMapping>.Some(seriesMapping));

            var aniDbSeriesData = new AniDbSeriesData().WithStandardData();
            var tvDbSeriesData = TvDbTestData.Series(142);

            var aniDbEpisodeData = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "33",
                    RawType = 1
                }
            };

            _tvDbClient.GetSeriesAsync(142).Returns(tvDbSeriesData);

            var dataMapper = new DataMapper(_mappingList, _tvDbClient, _episodeMatcher, _episodeMapper, _logManager);

            var episodeData = await dataMapper.MapEpisodeDataAsync(aniDbSeriesData, aniDbEpisodeData);

            episodeData.IsT0.Should().BeTrue();
            episodeData.AsT0.EpisodeData.Should().Be(aniDbEpisodeData);
        }

        //[Test]
        //[TestCase("3", 3)]
        //[TestCase("6", 6)]
        //[TestCase("4", 4)]
        //public async Task MapEpisodeDataAsync_AbsoluteTvDbSeason_FindsTvDbEpisodeWithMatchingAbsoluteIndex(
        //    string rawEpisodeNumber, int expectedTvDbEpisodeIndex)
        //{
        //    var mappingData = new MappingList(new[]
        //    {
        //        new SeriesMapping(
        //            new SeriesIds(324, 142, Option<int>.None, Option<int>.None),
        //            new AbsoluteTvDbSeason(),
        //            4,
        //            null,
        //            null)
        //    });

        //    var aniDbSeriesData = new AniDbSeriesData().WithStandardData();
        //    var aniDbEpisodeData = new AniDbEpisodeData
        //    {
        //        RawEpisodeNumber = new EpisodeNumberData
        //        {
        //            RawNumber = rawEpisodeNumber,
        //            RawType = 1
        //        }
        //    };

        //    var tvDbEpisodeData = TvDbTestData.Episode(123, absoluteEpisodeIndex: expectedTvDbEpisodeIndex);

        //    _tvDbClient.GetEpisodesAsync(142).Returns(new[] { tvDbEpisodeData }.ToList());

        //    var dataMapper = new DataMapper(_mappingList, _tvDbClient, _episodeMatcher, _episodeDataMapper, _logManager);

        //    var result = await dataMapper.MapEpisodeDataAsync(aniDbSeriesData, aniDbEpisodeData);

        //    result.IsT1.Should().BeTrue();

        //    result.Switch(
        //        x => { },
        //        combined =>
        //        {
        //            combined.AniDbEpisodeData.Should().Be(aniDbEpisodeData);
        //            combined.TvDbEpisodeData.Should().Be(tvDbEpisodeData);
        //            combined.FollowingTvDbEpisodeData.IsT2.Should().BeTrue();
        //        },
        //        x => { });
        //}

        //[Test]
        //public async Task MapEpisodeDataAsync_MappedSpecialEpisodeWithPosition_FindsFollowingTvDbEpisode()
        //{
        //    var mappingData = new MappingList(new[]
        //    {
        //        new SeriesMapping(new SeriesIds(324,44, Option<int>.None, Option<int>.None),
        //            new TvDbSeason(35), 4, new[]
        //            {
        //                new EpisodeGroupMapping(0, 35, 0, null, null, new[]
        //                {
        //                    new EpisodeMapping(3, 5),
        //                    new EpisodeMapping(66, 88)
        //                })
        //            }, new List<SpecialEpisodePosition>
        //            {
        //                new SpecialEpisodePosition(3, 66)
        //            })
        //    });

        //    var aniDbSeriesData = new AniDbSeriesData().WithStandardData();
        //    var aniDbEpisodeData = new AniDbEpisodeData
        //    {
        //        Id = 3,
        //        RawEpisodeNumber = new EpisodeNumberData
        //        {
        //            RawNumber = "3",
        //            RawType = 2
        //        }
        //    };
        //    var followingAniDbEpisodeData = new AniDbEpisodeData
        //    {
        //        Id = 66,
        //        RawEpisodeNumber = new EpisodeNumberData
        //        {
        //            RawNumber = "66",
        //            RawType = 2
        //        }
        //    };

        //    aniDbSeriesData.Episodes = new[]
        //    {
        //        aniDbEpisodeData,
        //        followingAniDbEpisodeData
        //    };

        //    var tvDbEpisodeData = TvDbTestData.Episode(2, 5, 35);
        //    var followingTvDbEpisodeData = TvDbTestData.Episode(1, 88, 35);

        //    _tvDbClient.GetEpisodesAsync(44).Returns(new[] { tvDbEpisodeData, followingTvDbEpisodeData }.ToList());

        //    var dataMapper = new DataMapper(_mappingList, _tvDbClient, _episodeMatcher, _episodeDataMapper, _logManager);

        //    var result = await dataMapper.MapEpisodeDataAsync(aniDbSeriesData, aniDbEpisodeData);

        //    result.IsT1.Should().BeTrue();

        //    result.Switch(x => { },
        //        combined =>
        //        {
        //            combined.AniDbEpisodeData.Should().Be(aniDbEpisodeData);
        //            combined.TvDbEpisodeData.Should().Be(tvDbEpisodeData);
        //            combined.FollowingTvDbEpisodeData.AsT1.TvDbEpisodeData.Should().Be(followingTvDbEpisodeData);
        //            combined.FollowingTvDbEpisodeData.AsT1.AniDbEpisodeData.Should().Be(followingAniDbEpisodeData);
        //        },
        //        x => { });
        //}

        //[Test]
        //[TestCase("3", 3, 9)]
        //[TestCase("6", 3, 12)]
        //[TestCase("4", 3, 10)]
        //public async Task MapEpisodeDataAsync_MatchingEpisodeGroupMapping_ReturnsMappedEpisodeNumber(
        //    string rawEpisodeNumber, int expectedTvDbSeasonIndex, int expectedTvDbEpisodeIndex)
        //{
        //    var mappingData = new MappingList(new[]
        //    {
        //        new SeriesMapping(new SeriesIds(1, 33, Option<int>.None, Option<int>.None),
        //            new TvDbSeason(35), 4, new[]
        //            {
        //                new EpisodeGroupMapping(1, 3, 6, 3, 6, null)
        //            }, null)
        //    });

        //    var aniDbSeriesData = new AniDbSeriesData().WithStandardData();
        //    var aniDbEpisodeData = new AniDbEpisodeData { Id = 3 };

        //    var tvDbEpisodeData = TvDbTestData.Episode(3, expectedTvDbEpisodeIndex, expectedTvDbSeasonIndex);

        //    _tvDbClient.GetEpisodesAsync(33).Returns(new[] { tvDbEpisodeData }.ToList());

        //    var dataMapper = new DataMapper(_mappingList, _tvDbClient, _episodeMatcher, _episodeDataMapper, _logManager);

        //    var result = await dataMapper.MapEpisodeDataAsync(aniDbSeriesData, aniDbEpisodeData);

        //    result.IsT1.Should().BeTrue();

        //    result.Switch(
        //        x => { },
        //        combined =>
        //        {
        //            combined.AniDbEpisodeData.Should().Be(aniDbEpisodeData);
        //            combined.TvDbEpisodeData.Should().Be(tvDbEpisodeData);
        //            combined.FollowingTvDbEpisodeData.IsT2.Should().BeTrue();
        //        },
        //        x => { });
        //}

        //[Test]
        //[TestCase("3", 3, 9)]
        //[TestCase("5", 3, 33)]
        //[TestCase("12", 3, 17)]
        //public async Task
        //    MapEpisodeDataAsync_MatchingEpisodeGroupMappingWithEpisodeMapping_ReturnsMappedEpisodeNumber(
        //        string rawEpisodeNumber, int expectedTvDbSeasonIndex, int expectedTvDbEpisodeIndex)
        //{
        //    var mappingData = new MappingList(new[]
        //    {
        //        new SeriesMapping(new SeriesIds(1, Option<int>.None, Option<int>.None, Option<int>.None),
        //            new TvDbSeason(35), 4, new[]
        //            {
        //                new EpisodeGroupMapping(1, 3, 6, 3, 6,
        //                    new[]
        //                    {
        //                        new EpisodeMapping(1, 5),
        //                        new EpisodeMapping(5, 33),
        //                        new EpisodeMapping(12, 17)
        //                    })
        //            }, null)
        //    });

        //    var dataMapper = new DataMapper(_mappingList, _tvDbClient, _episodeMatcher, _episodeDataMapper, _logManager);

        //    //var result =
        //    //    await dataMapper.GetMappedTvDbEpisodeIdAsync(1,
        //    //        new EpisodeNumberData { RawNumber = rawEpisodeNumber });

        //    //result.ResultType().Should().Be(typeof(MappedEpisodeIndexes));

        //    //result.Switch(tvDbEpisodeNumber =>
        //    //    {
        //    //        tvDbEpisodeNumber.EpisodeIndex.Should().Be(expectedTvDbEpisodeIndex);
        //    //        tvDbEpisodeNumber.SeasonIndex.Should().Be(expectedTvDbSeasonIndex);
        //    //    },
        //    //    x => { },
        //    //    x => { });
        //}

        //[Test]
        //public async Task MapEpisodeDataAsync_NoEpisodeGroupMapping_ReturnsDefaultEpisodeNumber()
        //{
        //    var mappingData = new MappingList(new[]
        //    {
        //        new SeriesMapping(new SeriesIds(1, Option<int>.None, Option<int>.None, Option<int>.None),
        //            new TvDbSeason(35), 4, null, null)
        //    });

        //    var dataMapper = new DataMapper(_mappingList, _tvDbClient, _episodeMatcher, _episodeDataMapper, _logManager);

        //    //var result = await dataMapper.GetMappedTvDbEpisodeIdAsync(1, new EpisodeNumberData { RawNumber = "3" });

        //    //result.ResultType().Should().Be(typeof(MappedEpisodeIndexes));

        //    //result.Switch(tvDbEpisodeNumber =>
        //    //    {
        //    //        tvDbEpisodeNumber.EpisodeIndex.Should().Be(7);
        //    //        tvDbEpisodeNumber.SeasonIndex.Should().Be(35);
        //    //    },
        //    //    x => { },
        //    //    x => { });
        //}

        //[Test]
        //public async Task MapEpisodeDataAsync_SpecialEpisodeWithPosition_ReturnsMappedFollowingTvDbId()
        //{
        //    var mappingData = new MappingList(new[]
        //    {
        //        new SeriesMapping(new SeriesIds(1, Option<int>.None, Option<int>.None, Option<int>.None),
        //            new TvDbSeason(35), 4, null, new List<SpecialEpisodePosition>
        //            {
        //                new SpecialEpisodePosition(3, 66)
        //            })
        //    });

        //    var dataMapper = new DataMapper(_mappingList, _tvDbClient, _episodeMatcher, _episodeDataMapper, _logManager);

        //    //var result = await dataMapper.GetMappedTvDbEpisodeIdAsync(1,
        //    //    new EpisodeNumberData { RawNumber = "3", RawType = 2 /* special */ });

        //    //result.ResultType().Should().Be(typeof(MappedEpisodeIndexes));

        //    //result.Switch(tvDbEpisodeNumber =>
        //    //    {
        //    //        tvDbEpisodeNumber.SeasonIndex.Should().Be(35);
        //    //        tvDbEpisodeNumber.EpisodeIndex.Should().Be(7);

        //    //        tvDbEpisodeNumber.FollowingTvDbEpisodeNumber.IsSome.Should().BeTrue();
        //    //        tvDbEpisodeNumber.FollowingTvDbEpisodeNumber.ValueUnsafe().SeasonIndex.Should().Be(35);
        //    //        tvDbEpisodeNumber.FollowingTvDbEpisodeNumber.ValueUnsafe().EpisodeIndex.Should().Be(70);
        //    //    },
        //    //    x => { },
        //    //    x => { });
        //}
    }
}