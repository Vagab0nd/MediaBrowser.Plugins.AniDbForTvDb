using System;
using System.Collections.Generic;
using FluentAssertions;
using Functional.Maybe;
using MediaBrowser.Plugins.Anime.AniDb.Mapping;
using MediaBrowser.Plugins.Anime.AniDb.Mapping.Data;
using NUnit.Framework;

namespace MediaBrowser.Plugins.Anime.Tests
{
    [TestFixture]
    public class MappingListTests
    {
        [Test]
        public void GetSeriesMapping_MatchingData_ReturnsSeriesMapping()
        {
            var mappingList = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(1, Maybe<int>.Nothing, Maybe<int>.Nothing, Maybe<int>.Nothing),
                    new TvDbSeasonResult(new TvDbSeason(5)), 1, null)
            });

            var result = mappingList.GetSeriesMapping(1);

            result.HasValue.Should().BeTrue();
            result.Value.Ids.AniDbSeriesId.Should().Be(1);
        }

        [Test]
        public void GetSeriesMapping_MultipleMatchingData_ThrowsException()
        {
            var mappingList = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(1, Maybe<int>.Nothing, Maybe<int>.Nothing, Maybe<int>.Nothing),
                    new TvDbSeasonResult(new TvDbSeason(5)), 1, null),
                new SeriesMapping(new SeriesIds(1, Maybe<int>.Nothing, Maybe<int>.Nothing, Maybe<int>.Nothing),
                    new TvDbSeasonResult(new TvDbSeason(5)), 1, null)
            });

            Action action = () => mappingList.GetSeriesMapping(1);

            action.ShouldThrow<Exception>();
        }

        [Test]
        public void GetSeriesMapping_NoData_ReturnsNone()
        {
            var mappingList = new MappingList(new List<SeriesMapping>());

            var result = mappingList.GetSeriesMapping(1);

            result.HasValue.Should().BeFalse();
        }

        [Test]
        public void GetSeriesMapping_NoMatchingData_ReturnsNone()
        {
            var mappingList = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(2, Maybe<int>.Nothing, Maybe<int>.Nothing, Maybe<int>.Nothing),
                    new TvDbSeasonResult(new TvDbSeason(5)), 1, null),
                new SeriesMapping(new SeriesIds(3, Maybe<int>.Nothing, Maybe<int>.Nothing, Maybe<int>.Nothing),
                    new TvDbSeasonResult(new TvDbSeason(5)), 1, null)
            });

            var result = mappingList.GetSeriesMapping(1);

            result.HasValue.Should().BeFalse();
        }

        [Test]
        public void GetSeriesMapping_NullData_ReturnsNone()
        {
            var mappingList = new MappingList(null);

            var result = mappingList.GetSeriesMapping(1);

            result.HasValue.Should().BeFalse();
        }

        [Test]
        public void FromData_NullData_ReturnsNone()
        {
            var mappingList = MappingList.FromData(null);

            mappingList.HasValue.Should().BeFalse();
        }

        [Test]
        public void FromData_NullMappings_ReturnsNone()
        {
            var mappingList = MappingList.FromData(new AnimeMappingListData());

            mappingList.HasValue.Should().BeFalse();
        }

        [Test]
        public void FromData_NonNullMappings_ReturnsMappingList()
        {
            var mappingList = MappingList.FromData(new AnimeMappingListData
            {
                AnimeSeriesMapping = new AniDbSeriesMappingData[] { }
            });

            mappingList.HasValue.Should().BeTrue();
        }
    }
}