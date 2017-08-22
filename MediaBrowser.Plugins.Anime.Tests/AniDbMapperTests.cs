using System.Collections.Generic;
using FluentAssertions;
using Functional.Maybe;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.Anime.AniDb.Mapping;
using MediaBrowser.Plugins.Anime.AniDb.Series.Data;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.Anime.Tests
{
    [TestFixture]
    public class AniDbMapperTests
    {
        private ILogManager _logManager;

        [SetUp]
        public void Setup()
        {
            _logManager = Substitute.For<ILogManager>();
        }

        [Test]
        public void GetMappedSeriesIds_MatchingMapping_ReturnsMappedIds()
        {
            var mappingData = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(1, 142.ToMaybe(), 124.ToMaybe(), 556.ToMaybe()),
                    new TvDbSeasonResult(new TvDbSeason(35)), 0, null)
            });

            var aniDbMapper = new AniDbMapper(mappingData, _logManager);

            var seriesIds = aniDbMapper.GetMappedSeriesIds(1);

            seriesIds.HasValue.Should().BeTrue();

            seriesIds.Value.TvDbSeriesId.Value.Should().Be(142);
            seriesIds.Value.ImdbSeriesId.Value.Should().Be(124);
            seriesIds.Value.TmDbSeriesId.Value.Should().Be(556);
        }

        [Test]
        public void GetMappedSeriesIds_NoMapping_ReturnsNone()
        {
            var aniDbMapper = new AniDbMapper(new MappingList(new List<SeriesMapping>()), _logManager);

            aniDbMapper.GetMappedSeriesIds(1).HasValue.Should().BeFalse();
        }

        [Test]
        public void GetMappedSeriesIds_SetImdbId_ReturnsImdbId()
        {
            var mappingData = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(1, Maybe<int>.Nothing, 523.ToMaybe(), Maybe<int>.Nothing),
                    new TvDbSeasonResult(new TvDbSeason(35)), 0, null)
            });

            var aniDbMapper = new AniDbMapper(mappingData, _logManager);

            var seriesIds = aniDbMapper.GetMappedSeriesIds(1);

            seriesIds.Value.ImdbSeriesId.HasValue.Should().BeTrue();
            seriesIds.Value.ImdbSeriesId.Value.Should().Be(523);
        }

        [Test]
        public void GetMappedSeriesIds_SetTmdbId_ReturnsTvDbId()
        {
            var mappingData = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(1, Maybe<int>.Nothing, Maybe<int>.Nothing, 677.ToMaybe()),
                    new TvDbSeasonResult(new TvDbSeason(35)), 0, null)
            });

            var aniDbMapper = new AniDbMapper(mappingData, _logManager);

            var seriesIds = aniDbMapper.GetMappedSeriesIds(1);

            seriesIds.Value.TmDbSeriesId.HasValue.Should().BeTrue();
            seriesIds.Value.TmDbSeriesId.Value.Should().Be(677);
        }

        [Test]
        public void GetMappedSeriesIds_SetTvDbId_ReturnsTvDbId()
        {
            var mappingData = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(1, 234.ToMaybe(), Maybe<int>.Nothing, Maybe<int>.Nothing),
                    new TvDbSeasonResult(new TvDbSeason(35)), 0, null)
            });

            var aniDbMapper = new AniDbMapper(mappingData, _logManager);

            var seriesIds = aniDbMapper.GetMappedSeriesIds(1);

            seriesIds.Value.TvDbSeriesId.HasValue.Should().BeTrue();
            seriesIds.Value.TvDbSeriesId.Value.Should().Be(234);
        }

        [Test]
        public void GetMappedSeriesIds_UnsetImdbId_ReturnsNoImdbId()
        {
            var mappingData = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(1, 5.ToMaybe(), Maybe<int>.Nothing, 5.ToMaybe()),
                    new TvDbSeasonResult(new TvDbSeason(35)), 0, null)
            });

            var aniDbMapper = new AniDbMapper(mappingData, _logManager);

            var seriesIds = aniDbMapper.GetMappedSeriesIds(1);

            seriesIds.Value.ImdbSeriesId.HasValue.Should().BeFalse();
        }

        [Test]
        public void GetMappedSeriesIds_UnsetTmdbId_ReturnsNoTmdbId()
        {
            var mappingData = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(1, 5.ToMaybe(), 5.ToMaybe(), Maybe<int>.Nothing),
                    new TvDbSeasonResult(new TvDbSeason(35)), 0, null)
            });

            var aniDbMapper = new AniDbMapper(mappingData, _logManager);

            var seriesIds = aniDbMapper.GetMappedSeriesIds(1);

            seriesIds.Value.TmDbSeriesId.HasValue.Should().BeFalse();
        }

        [Test]
        public void GetMappedSeriesIds_UnsetTvDbId_ReturnsNoTvDbId()
        {
            var mappingData = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(1, Maybe<int>.Nothing, 5.ToMaybe(), 5.ToMaybe()),
                    new TvDbSeasonResult(new TvDbSeason(35)), 0, null)
            });

            var aniDbMapper = new AniDbMapper(mappingData, _logManager);

            var seriesIds = aniDbMapper.GetMappedSeriesIds(1);

            seriesIds.Value.TvDbSeriesId.HasValue.Should().BeFalse();
        }

        [Test]
        public void GetMappedTvDbEpisodeId_NoEpisodeGroupMapping_ReturnsDefaultEpisodeNumber()
        {
            var mappingData = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(1, Maybe<int>.Nothing, Maybe<int>.Nothing, Maybe<int>.Nothing),
                    new TvDbSeasonResult(new TvDbSeason(35)), 4, null)
            });

            var aniDbMapper = new AniDbMapper(mappingData, _logManager);

            var result = aniDbMapper.GetMappedTvDbEpisodeId(1, new EpisodeNumberData { RawNumber = "3" });

            result.ResultType().Should().Be(typeof(TvDbEpisodeNumber));

            result.Match(tvDbEpisodeNumber =>
                {
                    tvDbEpisodeNumber.EpisodeIndex.Should().Be(7);
                    tvDbEpisodeNumber.SeasonIndex.Should().Be(35);
                },
                x => { },
                x => { });
        }

        [Test]
        public void GetMappedTvDbEpisodeId_NoMapping_ReturnsUnmappedEpisodeNumber()
        {
            var aniDbMapper = new AniDbMapper(new MappingList(null), _logManager);

            var episodeNumber = aniDbMapper.GetMappedTvDbEpisodeId(1, new EpisodeNumberData());

            episodeNumber.ResultType().Should().Be(typeof(UnmappedEpisodeNumber));
        }

        [Test]
        public void GetMappedTvDbEpisodeId_NullEpisodeNumber_ReturnsUnmappedEpisodeNumber()
        {
            var mappingData = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(1, Maybe<int>.Nothing, Maybe<int>.Nothing, Maybe<int>.Nothing),
                    new TvDbSeasonResult(new TvDbSeason(35)), 4, null)
            });

            var aniDbMapper = new AniDbMapper(mappingData, _logManager);

            var episodeNumber = aniDbMapper.GetMappedTvDbEpisodeId(1, null);

            episodeNumber.ResultType().Should().Be(typeof(UnmappedEpisodeNumber));
        }

        [Test]
        [TestCase("3", 3, 9)]
        [TestCase("6", 3, 12)]
        [TestCase("4", 3, 10)]
        public void GetMappedTvDbEpisodeId_MatchingEpisodeGroupMapping_ReturnsMappedEpisodeNumber(string rawEpisodeNumber, int expectedTvDbSeasonIndex, int expectedTvDbEpisodeIndex)
        {
            var mappingData = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(1, Maybe<int>.Nothing, Maybe<int>.Nothing, Maybe<int>.Nothing),
                    new TvDbSeasonResult(new TvDbSeason(35)), 4, new []
                    {
                        new EpisodeGroupMapping(1, 3, 6, 3, 6, null)
                    })
            });

            var aniDbMapper = new AniDbMapper(mappingData, _logManager);

            var result = aniDbMapper.GetMappedTvDbEpisodeId(1, new EpisodeNumberData { RawNumber = rawEpisodeNumber });

            result.ResultType().Should().Be(typeof(TvDbEpisodeNumber));

            result.Match(tvDbEpisodeNumber =>
                {
                    tvDbEpisodeNumber.EpisodeIndex.Should().Be(expectedTvDbEpisodeIndex);
                    tvDbEpisodeNumber.SeasonIndex.Should().Be(expectedTvDbSeasonIndex);
                },
                x => { },
                x => { });
        }

        [Test]
        [TestCase("3", 3)]
        [TestCase("6", 6)]
        [TestCase("4", 4)]
        public void GetMappedTvDbEpisodeId_AbsoluteTvDbSeason_ReturnsAbsoluteEpisodeNumber(string rawEpisodeNumber, int expectedTvDbEpisodeIndex)
        {
            var mappingData = new MappingList(new[]
            {
                new SeriesMapping(
                    new SeriesIds(1, Maybe<int>.Nothing, Maybe<int>.Nothing, Maybe<int>.Nothing),
                    new TvDbSeasonResult(new AbsoluteTvDbSeason()),
                    4,
                    null)
            });

            var aniDbMapper = new AniDbMapper(mappingData, _logManager);

            var result = aniDbMapper.GetMappedTvDbEpisodeId(1, new EpisodeNumberData { RawNumber = rawEpisodeNumber });

            result.ResultType().Should().Be(typeof(AbsoluteEpisodeNumber));

            result.Match(x => { },
                absoluteEpisodeNumber => absoluteEpisodeNumber.EpisodeIndex.Should().Be(expectedTvDbEpisodeIndex),
                x => { });
        }

        [Test]
        [TestCase("3", 3, 9)]
        [TestCase("5", 3, 33)]
        [TestCase("12", 3, 17)]
        public void GetMappedTvDbEpisodeId_MatchingEpisodeGroupMappingWithEpisodeMapping_ReturnsMappedEpisodeNumber(string rawEpisodeNumber, int expectedTvDbSeasonIndex, int expectedTvDbEpisodeIndex)
        {
            var mappingData = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(1, Maybe<int>.Nothing, Maybe<int>.Nothing, Maybe<int>.Nothing),
                    new TvDbSeasonResult(new TvDbSeason(35)), 4, new []
                    {
                        new EpisodeGroupMapping(1, 3, 6, 3, 6, 
                        new []
                        {
                            new EpisodeMapping(1, 5),
                            new EpisodeMapping(5, 33),
                            new EpisodeMapping(12, 17)
                        })
                    })
            });

            var aniDbMapper = new AniDbMapper(mappingData, _logManager);

            var result = aniDbMapper.GetMappedTvDbEpisodeId(1, new EpisodeNumberData { RawNumber = rawEpisodeNumber });

            result.ResultType().Should().Be(typeof(TvDbEpisodeNumber));

            result.Match(tvDbEpisodeNumber =>
                {
                    tvDbEpisodeNumber.EpisodeIndex.Should().Be(expectedTvDbEpisodeIndex);
                    tvDbEpisodeNumber.SeasonIndex.Should().Be(expectedTvDbSeasonIndex);
                },
                x => { },
                x => { });
        }

    }
}