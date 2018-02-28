using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Mapping;
using MediaBrowser.Plugins.AniMetadata.Providers;
using MediaBrowser.Plugins.AniMetadata.Providers.AniDb;
using MediaBrowser.Plugins.AniMetadata.Tests.TestData;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class DataMapperTests
    {
        [TestFixture]
        public class AniDbToTvDb : DataMapperTests
        {
            [SetUp]
            public void Setup()
            {
                _tvDbClient = Substitute.For<ITvDbClient>();
                _aniDbClient = Substitute.For<IAniDbClient>();
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
            private IAniDbClient _aniDbClient;

            [Test]
            public async Task MapAniDbEpisodeAsync_HasMappedEpisodeData_ReturnsMappedEpisodeData()
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

                _episodeMapper.MapAniDbEpisodeAsync(33, seriesMapping, Option<EpisodeGroupMapping>.None)
                    .Returns(tvDbEpisodeData);

                var dataMapper = new DataMapper(_mappingList, _tvDbClient, _aniDbClient, _episodeMatcher,
                    _episodeMapper, _logManager);

                var episodeData = await dataMapper.MapEpisodeDataAsync(aniDbSeriesData, aniDbEpisodeData);

                episodeData.IsT1.Should().BeTrue();
                episodeData.AsT1.AniDbEpisodeData.Should().Be(aniDbEpisodeData);
                episodeData.AsT1.TvDbEpisodeData.Should().Be(tvDbEpisodeData);
                episodeData.AsT1.FollowingTvDbEpisodeData.IsT2.Should().BeTrue();
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

                var dataMapper = new DataMapper(_mappingList, _tvDbClient, _aniDbClient, _episodeMatcher,
                    _episodeMapper, _logManager);

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

                var dataMapper = new DataMapper(_mappingList, _tvDbClient, _aniDbClient, _episodeMatcher,
                    _episodeMapper, _logManager);

                await dataMapper.MapEpisodeDataAsync(aniDbSeriesData, aniDbEpisodeData);

                seriesMapping.Received(1).GetSpecialEpisodePosition(aniDbEpisodeData.EpisodeNumber);
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

                var dataMapper = new DataMapper(_mappingList, _tvDbClient, _aniDbClient, _episodeMatcher,
                    _episodeMapper, _logManager);

                var episodeData = await dataMapper.MapEpisodeDataAsync(aniDbSeriesData, aniDbEpisodeData);

                episodeData.IsT0.Should().BeTrue();
                episodeData.AsT0.EpisodeData.Should().Be(aniDbEpisodeData);
            }

            [Test]
            public async Task MapEpisodeDataAsync_NoSeriesMapping_ReturnsAniDbEpisodeDataOnly()
            {
                var dataMapper = new DataMapper(_mappingList, _tvDbClient, _aniDbClient, _episodeMatcher,
                    _episodeMapper, _logManager);

                var episodeData = await dataMapper.MapEpisodeDataAsync(new AniDbSeriesData(), new AniDbEpisodeData());

                episodeData.IsT0.Should().BeTrue();
            }

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

                var dataMapper = new DataMapper(_mappingList, _tvDbClient, _aniDbClient, _episodeMatcher,
                    _episodeMapper, _logManager);

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

                var dataMapper = new DataMapper(_mappingList, _tvDbClient, _aniDbClient, _episodeMatcher,
                    _episodeMapper, _logManager);

                var result = await dataMapper.MapSeriesDataAsync(aniDbSeriesData);

                result.IsT0.Should().BeTrue();
                result.AsT0.AniDbSeriesData.Should().Be(aniDbSeriesData);
            }
        }

        [TestFixture]
        public class TvDbToAniDb : DataMapperTests
        {
            [SetUp]
            public void Setup()
            {
                _tvDbClient = Substitute.For<ITvDbClient>();
                _aniDbClient = Substitute.For<IAniDbClient>();
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
            private IAniDbClient _aniDbClient;

            [Test]
            public async Task MapAniDbEpisodeAsync_HasMappedEpisodeData_ReturnsMappedEpisodeData()
            {
                var seriesMapping = Substitute.For<ISeriesMapping>();
                seriesMapping.Ids.Returns(new SeriesIds(324, 142, Option<int>.None, Option<int>.None));

                _mappingList.GetSeriesMappingsFromTvDb(142)
                    .Returns(Option<IEnumerable<ISeriesMapping>>.Some(new[] { seriesMapping }));

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
                var tvDbEpisodeData = TvDbTestData.Episode(3, 44);

                _aniDbClient.GetSeriesAsync(142).Returns(aniDbSeriesData);

                _episodeMapper.MapTvDbEpisodeAsync(44, seriesMapping, Option<EpisodeGroupMapping>.None)
                    .Returns(aniDbEpisodeData);

                var dataMapper = new DataMapper(_mappingList, _tvDbClient, _aniDbClient, _episodeMatcher,
                    _episodeMapper, _logManager);

                var episodeData = await dataMapper.MapEpisodeDataAsync(324, tvDbSeriesData, tvDbEpisodeData);

                episodeData.IsT1.Should().BeTrue();
                episodeData.AsT1.AniDbEpisodeData.Should().Be(aniDbEpisodeData);
                episodeData.AsT1.TvDbEpisodeData.Should().Be(tvDbEpisodeData);
                episodeData.AsT1.FollowingTvDbEpisodeData.IsT2.Should().BeTrue();
            }

            [Test]
            public async Task MapEpisodeDataAsync_HasSeriesMapping_GetsEpisodeGroupMapping()
            {
                var seriesMapping = Substitute.For<ISeriesMapping>();
                seriesMapping.Ids.Returns(new SeriesIds(324, 142, Option<int>.None, Option<int>.None));

                _mappingList.GetSeriesMappingsFromTvDb(142).Returns(Option<ISeriesMapping>.Some(seriesMapping));

                var aniDbSeriesData = new AniDbSeriesData().WithStandardData();
                var tvDbSeriesData = TvDbTestData.Series(142);

                var tvDbEpisodeData = TvDbTestData.Episode(44, 12, 1);

                _aniDbClient.GetSeriesAsync(142).Returns(aniDbSeriesData);

                var dataMapper = new DataMapper(_mappingList, _tvDbClient, _aniDbClient, _episodeMatcher,
                    _episodeMapper, _logManager);

                await dataMapper.MapEpisodeDataAsync(324, tvDbSeriesData, tvDbEpisodeData);

                seriesMapping.Received(1).GetEpisodeGroupMapping(12, 1);
            }

            [Test]
            public async Task MapEpisodeDataAsync_NoMappedEpisodeData_ReturnsNoEpisodeData()
            {
                var seriesMapping = Substitute.For<ISeriesMapping>();
                seriesMapping.Ids.Returns(new SeriesIds(324, 142, Option<int>.None, Option<int>.None));

                _mappingList.GetSeriesMappingFromAniDb(142).Returns(Option<ISeriesMapping>.Some(seriesMapping));

                var aniDbSeriesData = new AniDbSeriesData().WithStandardData();
                var tvDbSeriesData = TvDbTestData.Series(142);

                var tvDbEpisodeData = TvDbTestData.Episode(44, 12, 1);

                _aniDbClient.GetSeriesAsync(142).Returns(aniDbSeriesData);

                var dataMapper = new DataMapper(_mappingList, _tvDbClient, _aniDbClient, _episodeMatcher,
                    _episodeMapper, _logManager);

                var episodeData = await dataMapper.MapEpisodeDataAsync(142, tvDbSeriesData, tvDbEpisodeData);

                episodeData.IsT2.Should().BeTrue();
            }

            [Test]
            public async Task MapEpisodeDataAsync_NoSeriesMapping_ReturnsNoEpisodeData()
            {
                var dataMapper = new DataMapper(_mappingList, _tvDbClient, _aniDbClient, _episodeMatcher,
                    _episodeMapper, _logManager);

                var episodeData = await dataMapper.MapEpisodeDataAsync(324, TvDbTestData.Series(142), TvDbTestData.Episode(44, 12, 1));

                episodeData.IsT2.Should().BeTrue();
            }

            [Test]
            public async Task MapSeriesDataAsync_MultipleMatchingMappings_ReturnsAllMappedSeries()
            {
                var seriesIdsA = new SeriesIds(324, 142, Option<int>.None, Option<int>.None);
                var seriesIdsB = new SeriesIds(111, 142, Option<int>.None, Option<int>.None);

                var seriesMappingA = Substitute.For<ISeriesMapping>();
                seriesMappingA.Ids.Returns(seriesIdsA);

                var seriesMappingB = Substitute.For<ISeriesMapping>();
                seriesMappingB.Ids.Returns(seriesIdsB);

                _mappingList.GetSeriesMappingsFromTvDb(142).Returns(Option<IEnumerable<ISeriesMapping>>.Some(new[] { seriesMappingA, seriesMappingB }));

                var aniDbSeriesDataA = new AniDbSeriesData().WithStandardData();
                var aniDbSeriesDataB = new AniDbSeriesData().WithStandardData();
                var tvDbSeriesData = TvDbTestData.Series(142);

                _aniDbClient.GetSeriesAsync(324).Returns(aniDbSeriesDataA);
                _aniDbClient.GetSeriesAsync(111).Returns(aniDbSeriesDataB);

                var dataMapper = new DataMapper(_mappingList, _tvDbClient, _aniDbClient, _episodeMatcher,
                    _episodeMapper, _logManager);

                var seriesData = await dataMapper.MapSeriesDataAsync(tvDbSeriesData);

                seriesData.Select(sd => sd.AsT1)
                    .Should()
                    .BeEquivalentTo(new CombinedSeriesData(seriesIdsA, aniDbSeriesDataA, tvDbSeriesData), new CombinedSeriesData(seriesIdsB, aniDbSeriesDataB, tvDbSeriesData));
            }

            [Test]
            public async Task MapSeriesDataAsync_SingleMatchingMapping_ReturnsMappedSeries()
            {
                var seriesIds = new SeriesIds(324, 142, Option<int>.None, Option<int>.None);

                var seriesMapping = Substitute.For<ISeriesMapping>();
                seriesMapping.Ids.Returns(seriesIds);

                _mappingList.GetSeriesMappingsFromTvDb(142).Returns(Option<IEnumerable<ISeriesMapping>>.Some(new[] { seriesMapping }));

                var aniDbSeriesData = new AniDbSeriesData().WithStandardData();
                var tvDbSeriesData = TvDbTestData.Series(142);

                _aniDbClient.GetSeriesAsync(324).Returns(aniDbSeriesData);

                var dataMapper = new DataMapper(_mappingList, _tvDbClient, _aniDbClient, _episodeMatcher,
                    _episodeMapper, _logManager);

                var seriesData = await dataMapper.MapSeriesDataAsync(tvDbSeriesData);

                seriesData.Select(sd => sd.AsT1)
                    .Should()
                    .BeEquivalentTo(new CombinedSeriesData(seriesIds, aniDbSeriesData, tvDbSeriesData));
            }

            [Test]
            public async Task MapSeriesDataAsync_NoMatchingMapping_ReturnsEmpty()
            {
                _mappingList.GetSeriesMappingsFromTvDb(142).Returns(Option<IEnumerable<ISeriesMapping>>.Some(new ISeriesMapping[] { }));

                var tvDbSeriesData = TvDbTestData.Series(142);

                var dataMapper = new DataMapper(_mappingList, _tvDbClient, _aniDbClient, _episodeMatcher,
                    _episodeMapper, _logManager);

                var seriesData = await dataMapper.MapSeriesDataAsync(tvDbSeriesData);

                seriesData.Should().BeEmpty();
            }
        }
    }
}