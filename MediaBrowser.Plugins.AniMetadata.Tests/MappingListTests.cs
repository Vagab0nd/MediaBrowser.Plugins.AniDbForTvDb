using System;
using System.Collections.Generic;
using FluentAssertions;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping;
using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping.Data;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class MappingListTests
    {
        [Test]
        public void FromData_NonNullMappings_ReturnsMappingList()
        {
            var mappingList = MappingList.FromData(new AnimeMappingListData
            {
                AnimeSeriesMapping = new AniDbSeriesMappingData[] { }
            });

            mappingList.IsSome.Should().BeTrue();
        }

        [Test]
        public void FromData_NullData_ReturnsNone()
        {
            var mappingList = MappingList.FromData(null);

            mappingList.IsSome.Should().BeFalse();
        }

        [Test]
        public void FromData_NullMappings_ReturnsNone()
        {
            var mappingList = MappingList.FromData(new AnimeMappingListData());

            mappingList.IsSome.Should().BeFalse();
        }

        [Test]
        public void GetSeriesMapping_MatchingData_ReturnsSeriesMapping()
        {
            var mappingList = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(1, Option<int>.None, Option<int>.None, Option<int>.None),
                    new TvDbSeason(5), 1, null, null)
            });

            var result = mappingList.GetSeriesMapping(1);

            result.IsSome.Should().BeTrue();
            result.ValueUnsafe().Ids.AniDbSeriesId.Should().Be(1);
        }

        [Test]
        public void GetSeriesMapping_MultipleMatchingData_ThrowsException()
        {
            var mappingList = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(1, Option<int>.None, Option<int>.None, Option<int>.None),
                    new TvDbSeason(5), 1, null, null),
                new SeriesMapping(new SeriesIds(1, Option<int>.None, Option<int>.None, Option<int>.None),
                    new TvDbSeason(5), 1, null, null)
            });

            Action action = () => mappingList.GetSeriesMapping(1);

            action.ShouldThrow<Exception>();
        }

        [Test]
        public void GetSeriesMapping_NoData_ReturnsNone()
        {
            var mappingList = new MappingList(new List<SeriesMapping>());

            var result = mappingList.GetSeriesMapping(1);

            result.IsSome.Should().BeFalse();
        }

        [Test]
        public void GetSeriesMapping_NoMatchingData_ReturnsNone()
        {
            var mappingList = new MappingList(new[]
            {
                new SeriesMapping(new SeriesIds(2, Option<int>.None, Option<int>.None, Option<int>.None),
                    new TvDbSeason(5), 1, null, null),
                new SeriesMapping(new SeriesIds(3, Option<int>.None, Option<int>.None, Option<int>.None),
                    new TvDbSeason(5), 1, null, null)
            });

            var result = mappingList.GetSeriesMapping(1);

            result.IsSome.Should().BeFalse();
        }

        [Test]
        public void GetSeriesMapping_NullData_ReturnsNone()
        {
            var mappingList = new MappingList(null);

            var result = mappingList.GetSeriesMapping(1);

            result.IsSome.Should().BeFalse();
        }
    }
}