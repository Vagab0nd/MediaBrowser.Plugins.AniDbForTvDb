using System.Linq;
using FluentAssertions;
using LanguageExt.UnsafeValueAccess;
using MediaBrowser.Plugins.AniMetadata.Mapping;
using MediaBrowser.Plugins.AniMetadata.Mapping.Data;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class EpisodeGroupMappingTests
    {
        [Test]
        [TestCase(4)]
        [TestCase(10)]
        [TestCase(16)]
        public void CanMapEpisode_BetweenStartAndEnd_ReturnsTrue(int episodeIndex)
        {
            var episodeGroupMapping = new EpisodeGroupMapping(1, 1, 1, 4, 16, null);

            episodeGroupMapping.CanMapEpisode(episodeIndex).Should().BeTrue();
        }

        [Test]
        public void CanMapEpisode_MatchesEpisodeMapping_ReturnsTrue()
        {
            var episodeGroupMapping = new EpisodeGroupMapping(1, 1, 1, null, null, new[]
            {
                new EpisodeMapping(4, 12),
                new EpisodeMapping(6, 33)
            });

            episodeGroupMapping.CanMapEpisode(6).Should().BeTrue();
        }

        [Test]
        public void CanMapEpisode_NoMatch_ReturnsFalse()
        {
            var episodeGroupMapping = new EpisodeGroupMapping(1, 1, 1, 4, 16, new[]
            {
                new EpisodeMapping(4, 12),
                new EpisodeMapping(6, 33)
            });

            episodeGroupMapping.CanMapEpisode(17).Should().BeFalse();
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("4444")]
        [TestCase(";;;;")]
        [TestCase(";1;4;")]
        public void Create_InvalidEpisodeMappingString_EmptyEpisodeMappings(string episodeMappingString)
        {
            var data = new AnimeEpisodeGroupMappingData
            {
                EpisodeMappingString = episodeMappingString
            };

            var episodeGroupMapping = EpisodeGroupMapping.FromData(data);

            episodeGroupMapping.ValueUnsafe().EpisodeMappings.Should().BeEmpty();
        }

        [Test]
        public void Create_NonNullData_SetsAniDbSeasonIndex()
        {
            var data = new AnimeEpisodeGroupMappingData
            {
                AnidbSeason = 44
            };

            var episodeGroupMapping = EpisodeGroupMapping.FromData(data);

            episodeGroupMapping.ValueUnsafe().AniDbSeasonIndex.Should().Be(44);
        }

        [Test]
        public void Create_NonNullData_SetsTvDbEpisodeIndexOffset()
        {
            var data = new AnimeEpisodeGroupMappingData
            {
                Offset = 12
            };

            var episodeGroupMapping = EpisodeGroupMapping.FromData(data);

            episodeGroupMapping.ValueUnsafe().TvDbEpisodeIndexOffset.Should().Be(12);
        }

        [Test]
        public void Create_NonNullData_SetsTvDbSeasonIndex()
        {
            var data = new AnimeEpisodeGroupMappingData
            {
                TvDbSeason = 443
            };

            var episodeGroupMapping = EpisodeGroupMapping.FromData(data);

            episodeGroupMapping.ValueUnsafe().TvDbSeasonIndex.Should().Be(443);
        }

        [Test]
        public void Create_NullData_ReturnsNone()
        {
            var episodeGroupMapping = EpisodeGroupMapping.FromData(null);

            episodeGroupMapping.IsSome.Should().BeFalse();
        }

        [Test]
        public void Create_OneEpisodeMappingString_CreatesOneEpisodeMapping()
        {
            var data = new AnimeEpisodeGroupMappingData
            {
                EpisodeMappingString = ";5-3;"
            };

            var episodeGroupMapping = EpisodeGroupMapping.FromData(data);

            episodeGroupMapping.ValueUnsafe().EpisodeMappings.Should().HaveCount(1);

            var episodeMapping = episodeGroupMapping.ValueUnsafe().EpisodeMappings.Single();

            episodeMapping.AniDbEpisodeIndex.Should().Be(5);
            episodeMapping.TvDbEpisodeIndex.Should().Be(3);
        }

        [Test]
        public void Create_TwoEpisodeMappingStrings_CreatesTwoEpisodeMapping()
        {
            var data = new AnimeEpisodeGroupMappingData
            {
                EpisodeMappingString = ";5-3;22-55;"
            };

            var episodeGroupMapping = EpisodeGroupMapping.FromData(data);

            episodeGroupMapping.ValueUnsafe().EpisodeMappings.Should().HaveCount(2);

            var episodeMapping = episodeGroupMapping.ValueUnsafe().EpisodeMappings.First();

            episodeMapping.AniDbEpisodeIndex.Should().Be(5);
            episodeMapping.TvDbEpisodeIndex.Should().Be(3);

            episodeMapping = episodeGroupMapping.ValueUnsafe().EpisodeMappings.Last();

            episodeMapping.AniDbEpisodeIndex.Should().Be(22);
            episodeMapping.TvDbEpisodeIndex.Should().Be(55);
        }
    }
}