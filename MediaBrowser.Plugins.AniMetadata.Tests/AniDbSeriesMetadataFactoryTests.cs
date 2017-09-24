using System;
using System.Collections.Generic;
using FluentAssertions;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.MetadataMapping;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class AniDbSeriesMetadataFactoryTests
    {
        [SetUp]
        public void Setup()
        {
            _metadataMapping = Substitute.For<IMetadataMapping>();
        }

        private IMetadataMapping _metadataMapping;

        [Test]
        public void CreateMetadata_HasTitle_ReturnsPopulatedSeries()
        {
            var series = new AniDbSeriesData
            {
                Id = 44
            };

            var expectedResult = new MetadataResult<Series>
            {
                Item = new Series
                {
                    Name = "Name"
                }
            };

            _metadataMapping.Apply(series, Arg.Any<MetadataResult<Series>>())
                .Returns(expectedResult);

            var metadataFactory = new AniDbSeriesMetadataFactory(_metadataMapping);

            metadataFactory.CreateMetadata(series).Should().Be(expectedResult);
        }

        [Test]
        public void CreateMetadata_MultipleSources_HasTitle_ReturnsPopulatedSeries()
        {
            var aniDbSeries = new AniDbSeriesData
            {
                Id = 44
            };
            var tvDbSeries = new TvDbSeriesData(66, "", null, "", 0, DayOfWeek.Monday, "", 0, new List<string>(),
                new List<string>(), "");

            var expectedResult = new MetadataResult<Series>
            {
                Item = new Series
                {
                    Name = "Name"
                }
            };

            _metadataMapping.Apply(Arg.Is<object[]>(a => a[0] == aniDbSeries && a[1] == tvDbSeries),
                    Arg.Any<MetadataResult<Series>>())
                .Returns(expectedResult);

            var metadataFactory = new AniDbSeriesMetadataFactory(_metadataMapping);

            metadataFactory.CreateMetadata(aniDbSeries, tvDbSeries).Should().Be(expectedResult);
        }

        [Test]
        public void CreateMetadata_MultipleSources_NoTitle_ReturnsNullResult()
        {
            var aniDbSeries = new AniDbSeriesData
            {
                Id = 44
            };
            var tvDbSeries = new TvDbSeriesData(66, "", null, "", 0, DayOfWeek.Monday, "", 0, new List<string>(),
                new List<string>(), "");

            _metadataMapping.Apply(Arg.Is<object[]>(a => a[0] == aniDbSeries && a[1] == tvDbSeries),
                    Arg.Any<MetadataResult<Series>>())
                .Returns(new MetadataResult<Series>
                {
                    Item = new Series
                    {
                        Name = ""
                    }
                });

            var metadataFactory = new AniDbSeriesMetadataFactory(_metadataMapping);

            metadataFactory.CreateMetadata(aniDbSeries, tvDbSeries).ShouldBeEquivalentTo(metadataFactory.NullResult);
        }

        [Test]
        public void CreateMetadata_NoTitle_ReturnsNullResult()
        {
            var series = new AniDbSeriesData
            {
                Titles = new ItemTitleData[0]
            };

            _metadataMapping.Apply(series, Arg.Any<MetadataResult<Series>>())
                .Returns(new MetadataResult<Series>
                {
                    Item = new Series
                    {
                        Name = ""
                    }
                });

            var metadataFactory = new AniDbSeriesMetadataFactory(_metadataMapping);

            metadataFactory.CreateMetadata(series).ShouldBeEquivalentTo(metadataFactory.NullResult);
        }
    }
}