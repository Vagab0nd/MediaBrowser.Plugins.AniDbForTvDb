using System.Linq;
using FluentAssertions;
using MediaBrowser.Plugins.Anime.AniDb.Mapping;
using MediaBrowser.Plugins.Anime.AniDb.Mapping.Data;
using MediaBrowser.Plugins.Anime.Tests.TestHelpers;
using NUnit.Framework;

namespace MediaBrowser.Plugins.Anime.Tests
{
    [TestFixture]
    public class SeriesMappingTests
    {
        [Test]
        public void FromData_AbsoluteTvDbSeason_ReturnsAbsoluteTvDbSeason()
        {
            var data = new AniDbSeriesMappingData
            {
                AnidbId = "3",
                DefaultTvDbSeason = "a"
            };

            var seriesMapping = SeriesMapping.FromData(data);

            seriesMapping.HasValue.Should().BeTrue();
            seriesMapping.Value.DefaultTvDbSeason.ResultType().Should().Be(typeof(AbsoluteTvDbSeason));
        }

        [Test]
        public void FromData_IntegerAniDbId_ReturnsAniDbSeriesId()
        {
            var data = new AniDbSeriesMappingData
            {
                AnidbId = "3",
                DefaultTvDbSeason = "a"
            };

            var seriesMapping = SeriesMapping.FromData(data);

            seriesMapping.HasValue.Should().BeTrue();
            seriesMapping.Value.Ids.AniDbSeriesId.Should().Be(3);
        }

        [Test]
        public void FromData_IntegerTvDbSeason_ReturnsTvDbSeasonIndex()
        {
            var data = new AniDbSeriesMappingData
            {
                AnidbId = "3",
                DefaultTvDbSeason = "35"
            };

            var seriesMapping = SeriesMapping.FromData(data);

            seriesMapping.HasValue.Should().BeTrue();
            seriesMapping.Value.DefaultTvDbSeason.ResultType().Should().Be(typeof(TvDbSeason));

            var tvDbSeasonIndex = -1;

            seriesMapping.Value.DefaultTvDbSeason.Match(s => tvDbSeasonIndex = s.Index, s => { });
            tvDbSeasonIndex.Should().Be(35);
        }

        [Test]
        [TestCase(null)]
        [TestCase("And")]
        [TestCase("")]
        [TestCase("999999999999")]
        public void FromData_InvalidAniDbId_ReturnsNone(string aniDbId)
        {
            var data = new AniDbSeriesMappingData
            {
                AnidbId = aniDbId,
                DefaultTvDbSeason = "a"
            };

            var seriesMapping = SeriesMapping.FromData(data);

            seriesMapping.HasValue.Should().BeFalse();
        }

        [Test]
        [TestCase(null)]
        [TestCase("And")]
        [TestCase("")]
        [TestCase("999999999999")]
        public void FromData_InvalidTvDbSeason_ReturnsNone(string tvDbSeason)
        {
            var data = new AniDbSeriesMappingData
            {
                AnidbId = "3",
                DefaultTvDbSeason = tvDbSeason
            };

            var seriesMapping = SeriesMapping.FromData(data);

            seriesMapping.HasValue.Should().BeFalse();
        }

        [Test]
        public void FromData_DefaultTvDbEpisodeIndexOffset_ReturnsValue()
        {
            var data = new AniDbSeriesMappingData
            {
                AnidbId = "3",
                DefaultTvDbSeason = "35",
                EpisodeOffset = 55
            };

            var seriesMapping = SeriesMapping.FromData(data);

            seriesMapping.HasValue.Should().BeTrue();
            seriesMapping.Value.DefaultTvDbEpisodeIndexOffset.Should().Be(55);
        }

        [Test]
        public void FromData_NullData_ReturnsNone()
        {
            var seriesMapping = SeriesMapping.FromData(null);

            seriesMapping.HasValue.Should().BeFalse();
        }

        [Test]
        public void FromData_NullGroupMappingList_SubstitutesEmptyList()
        {
            var data = new AniDbSeriesMappingData
            {
                AnidbId = "3",
                DefaultTvDbSeason = "35"
            };

            var seriesMapping = SeriesMapping.FromData(data);

            seriesMapping.Value.EpisodeGroupMappings.Should().BeEmpty();
        }

        [Test]
        public void FromData_EmptyGroupMappingList_CreatesNoEpisodeGroupMappings()
        {
            var data = new AniDbSeriesMappingData
            {
                AnidbId = "3",
                DefaultTvDbSeason = "35",
                GroupMappingList = new AnimeEpisodeGroupMappingData[0]
            };

            var seriesMapping = SeriesMapping.FromData(data);

            seriesMapping.Value.EpisodeGroupMappings.Should().BeEmpty();
        }

        [Test]
        public void FromData_NullItemsGroupMappingList_DoesNotCreateEpisodeGroupMapping()
        {
            var data = new AniDbSeriesMappingData
            {
                AnidbId = "3",
                DefaultTvDbSeason = "35",
                GroupMappingList = new AnimeEpisodeGroupMappingData[5]
            };

            var seriesMapping = SeriesMapping.FromData(data);

            seriesMapping.Value.EpisodeGroupMappings.Should().BeEmpty();
        }

        [Test]
        public void FromData_OneEpisodeGroupMapping_CreatesEpisodeGroupMapping()
        {
            var data = new AniDbSeriesMappingData
            {
                AnidbId = "3",
                DefaultTvDbSeason = "35",
                GroupMappingList = new []
                {
                    new AnimeEpisodeGroupMappingData()
                }
            };

            var seriesMapping = SeriesMapping.FromData(data);

            seriesMapping.Value.EpisodeGroupMappings.Should().HaveCount(1);
        }

        [Test]
        public void FromData_MultipleEpisodeGroupMappings_CreatesEpisodeGroupMappings()
        {
            var data = new AniDbSeriesMappingData
            {
                AnidbId = "3",
                DefaultTvDbSeason = "35",
                GroupMappingList = new[]
                {
                    new AnimeEpisodeGroupMappingData(),
                    new AnimeEpisodeGroupMappingData()
                }
            };

            var seriesMapping = SeriesMapping.FromData(data);

            seriesMapping.Value.EpisodeGroupMappings.Should().HaveCount(2);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("4444")]
        [TestCase(";;;;")]
        [TestCase(";1;4;")]
        public void Create_InvalidSpecialEpisodePositionsString_EmptySpecialEpisodePositions(string SpecialEpisodePositionsString)
        {
            var data = new AniDbSeriesMappingData
            {
                AnidbId = "3",
                DefaultTvDbSeason = "35",
                SpecialEpisodePositionsString = SpecialEpisodePositionsString
            };

            var seriesMapping = SeriesMapping.FromData(data);

            seriesMapping.Value.SpecialEpisodePositions.Should().BeEmpty();
        }

        [Test]
        public void Create_OneSpecialEpisodePositionsString_CreatesOneSpecialEpisodePosition()
        {
            var data = new AniDbSeriesMappingData
            {
                AnidbId = "3",
                DefaultTvDbSeason = "35",
                SpecialEpisodePositionsString = ";5-3;"
            };

            var seriesMapping = SeriesMapping.FromData(data);

            seriesMapping.Value.SpecialEpisodePositions.Should().HaveCount(1);

            var episodeMapping = seriesMapping.Value.SpecialEpisodePositions.Single();

            episodeMapping.SpecialEpisodeIndex.Should().Be(5);
            episodeMapping.FollowingStandardEpisodeIndex.Should().Be(3);
        }

        [Test]
        public void Create_TwoSpecialEpisodePositionsStrings_CreatesTwoSpecialEpisodePositions()
        {
            var data = new AniDbSeriesMappingData
            {
                AnidbId = "3",
                DefaultTvDbSeason = "35",
                SpecialEpisodePositionsString = ";5-3;22-55;"
            };

            var seriesMapping = SeriesMapping.FromData(data);

            seriesMapping.Value.SpecialEpisodePositions.Should().HaveCount(2);

            var episodeMapping = seriesMapping.Value.SpecialEpisodePositions.First();

            episodeMapping.SpecialEpisodeIndex.Should().Be(5);
            episodeMapping.FollowingStandardEpisodeIndex.Should().Be(3);

            episodeMapping = seriesMapping.Value.SpecialEpisodePositions.Last();

            episodeMapping.SpecialEpisodeIndex.Should().Be(22);
            episodeMapping.FollowingStandardEpisodeIndex.Should().Be(55);
        }
    }
}