using FluentAssertions;
using MediaBrowser.Plugins.Anime.AniDb.Mapping;
using MediaBrowser.Plugins.Anime.AniDb.Mapping.Data;
using NUnit.Framework;

namespace MediaBrowser.Plugins.Anime.Tests
{
    [TestFixture]
    public class AniDbMapperTests
    {
        [Test]
        public void GetMappedSeriesIds_MatchingMapping_ReturnsMappedIds()
        {
            var mappingData = new AnimeMappingListData
            {
                AnimeSeriesMapping = new[]
                {
                    new AniDbSeriesMappingData
                    {
                        AnidbId = "1",
                        TvDbId = "142",
                        ImdbId = "124",
                        TmdbId = "556"
                    }
                }
            };

            var aniDbMapper = new AniDbMapper(mappingData);

            var seriesIds = aniDbMapper.GetMappedSeriesIds(1);

            seriesIds.HasValue.Should().BeTrue();

            seriesIds.Value.TvDbSeriesId.Value.Should().Be(142);
            seriesIds.Value.ImdbSeriesId.Value.Should().Be(124);
            seriesIds.Value.TmDbSeriesId.Value.Should().Be(556);
        }

        [Test]
        public void GetMappedSeriesIds_NoMapping_ReturnsNone()
        {
            var aniDbMapper = new AniDbMapper(new AnimeMappingListData());

            aniDbMapper.GetMappedSeriesIds(1).HasValue.Should().BeFalse();
        }

        [Test]
        [TestCase("1", 1)]
        [TestCase("666", 666)]
        [TestCase("-543", -543)]
        [TestCase("999999999", 999999999)]
        public void GetMappedSeriesIds_ParsableImdbId_ReturnsTvDbId(string rawImdbId, int expectedImdbId)
        {
            var mappingData = new AnimeMappingListData
            {
                AnimeSeriesMapping = new[]
                {
                    new AniDbSeriesMappingData
                    {
                        AnidbId = "1",
                        ImdbId = rawImdbId
                    }
                }
            };

            var aniDbMapper = new AniDbMapper(mappingData);

            var seriesIds = aniDbMapper.GetMappedSeriesIds(1);

            seriesIds.Value.ImdbSeriesId.HasValue.Should().BeTrue();
            seriesIds.Value.ImdbSeriesId.Value.Should().Be(expectedImdbId);
        }

        [Test]
        [TestCase("1", 1)]
        [TestCase("666", 666)]
        [TestCase("-543", -543)]
        [TestCase("999999999", 999999999)]
        public void GetMappedSeriesIds_ParsableTmdbId_ReturnsTvDbId(string rawTmdbId, int expectedTmdbId)
        {
            var mappingData = new AnimeMappingListData
            {
                AnimeSeriesMapping = new[]
                {
                    new AniDbSeriesMappingData
                    {
                        AnidbId = "1",
                        TmdbId = rawTmdbId
                    }
                }
            };

            var aniDbMapper = new AniDbMapper(mappingData);

            var seriesIds = aniDbMapper.GetMappedSeriesIds(1);

            seriesIds.Value.TmDbSeriesId.HasValue.Should().BeTrue();
            seriesIds.Value.TmDbSeriesId.Value.Should().Be(expectedTmdbId);
        }

        [Test]
        [TestCase("1", 1)]
        [TestCase("666", 666)]
        [TestCase("-543", -543)]
        [TestCase("999999999", 999999999)]
        public void GetMappedSeriesIds_ParsableTvDbId_ReturnsTvDbId(string rawTvDbId, int expectedTvDbId)
        {
            var mappingData = new AnimeMappingListData
            {
                AnimeSeriesMapping = new[]
                {
                    new AniDbSeriesMappingData
                    {
                        AnidbId = "1",
                        TvDbId = rawTvDbId
                    }
                }
            };

            var aniDbMapper = new AniDbMapper(mappingData);

            var seriesIds = aniDbMapper.GetMappedSeriesIds(1);

            seriesIds.Value.TvDbSeriesId.HasValue.Should().BeTrue();
            seriesIds.Value.TvDbSeriesId.Value.Should().Be(expectedTvDbId);
        }

        [Test]
        [TestCase("")]
        [TestCase("Five")]
        [TestCase("£~{>><@})")]
        [TestCase("99999999999999999")]
        [TestCase(null)]
        public void GetMappedSeriesIds_UnparsableImdbId_ReturnsNoTvDbId(string rawImdbId)
        {
            var mappingData = new AnimeMappingListData
            {
                AnimeSeriesMapping = new[]
                {
                    new AniDbSeriesMappingData
                    {
                        AnidbId = "1",
                        ImdbId = rawImdbId
                    }
                }
            };

            var aniDbMapper = new AniDbMapper(mappingData);

            var seriesIds = aniDbMapper.GetMappedSeriesIds(1);

            seriesIds.Value.ImdbSeriesId.HasValue.Should().BeFalse();
        }

        [Test]
        [TestCase("")]
        [TestCase("Five")]
        [TestCase("£~{>><@})")]
        [TestCase("99999999999999999")]
        [TestCase(null)]
        public void GetMappedSeriesIds_UnparsableTmdbId_ReturnsNoTvDbId(string rawTmdbId)
        {
            var mappingData = new AnimeMappingListData
            {
                AnimeSeriesMapping = new[]
                {
                    new AniDbSeriesMappingData
                    {
                        AnidbId = "1",
                        TmdbId = rawTmdbId
                    }
                }
            };

            var aniDbMapper = new AniDbMapper(mappingData);

            var seriesIds = aniDbMapper.GetMappedSeriesIds(1);

            seriesIds.Value.TmDbSeriesId.HasValue.Should().BeFalse();
        }

        [Test]
        [TestCase("")]
        [TestCase("Five")]
        [TestCase("£~{>><@})")]
        [TestCase("99999999999999999")]
        [TestCase(null)]
        public void GetMappedSeriesIds_UnparsableTvDbId_ReturnsNoTvDbId(string rawTvDbId)
        {
            var mappingData = new AnimeMappingListData
            {
                AnimeSeriesMapping = new[]
                {
                    new AniDbSeriesMappingData
                    {
                        AnidbId = "1",
                        TvDbId = rawTvDbId
                    }
                }
            };

            var aniDbMapper = new AniDbMapper(mappingData);

            var seriesIds = aniDbMapper.GetMappedSeriesIds(1);

            seriesIds.Value.TvDbSeriesId.HasValue.Should().BeFalse();
        }
    }
}