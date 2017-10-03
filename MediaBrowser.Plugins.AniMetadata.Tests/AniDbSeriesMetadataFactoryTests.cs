using System;
using System.Collections.Generic;
using FluentAssertions;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.PropertyMapping;
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
            _propertyMappingCollection = Substitute.For<IPropertyMappingCollection>();

            _pluginConfiguration = Substitute.For<IPluginConfiguration>();
            _pluginConfiguration.GetSeriesMetadataMapping("en").Returns(_propertyMappingCollection);
        }

        private IPropertyMappingCollection _propertyMappingCollection;
        private IPluginConfiguration _pluginConfiguration;

        [Test]
        public void CreateMetadata_HasTitle_ReturnsPopulatedSeries()
        {
            var series = new AniDbSeriesData
            {
                Id = 44
            };

            var expectedResult = new MetadataResult<Series>
            {
                HasMetadata = true,
                Item = new Series
                {
                    Name = "Name"
                }
            };

            _propertyMappingCollection
                .Apply(series, Arg.Is<MetadataResult<Series>>(r => r.HasMetadata && r.Item != null))
                .Returns(expectedResult);

            var metadataFactory = new AniDbSeriesMetadataFactory(_pluginConfiguration);

            metadataFactory.CreateMetadata(series, "en").Should().Be(expectedResult);
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
                HasMetadata = true,
                Item = new Series
                {
                    Name = "Name"
                }
            };

            _propertyMappingCollection.Apply(Arg.Is<object[]>(a => a[0] == aniDbSeries && a[1] == tvDbSeries),
                    Arg.Any<MetadataResult<Series>>())
                .Returns(expectedResult);

            var metadataFactory = new AniDbSeriesMetadataFactory(_pluginConfiguration);

            metadataFactory.CreateMetadata(aniDbSeries, tvDbSeries, "en").Should().Be(expectedResult);
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

            _propertyMappingCollection.Apply(Arg.Is<object[]>(a => a[0] == aniDbSeries && a[1] == tvDbSeries),
                    Arg.Any<MetadataResult<Series>>())
                .Returns(new MetadataResult<Series>
                {
                    HasMetadata = false,
                    Item = new Series
                    {
                        Name = ""
                    }
                });

            var metadataFactory = new AniDbSeriesMetadataFactory(_pluginConfiguration);

            metadataFactory.CreateMetadata(aniDbSeries, tvDbSeries, "en").ShouldBeEquivalentTo(metadataFactory.NullResult);
        }

        [Test]
        public void CreateMetadata_NoTitle_ReturnsNullResult()
        {
            var series = new AniDbSeriesData
            {
                Titles = new ItemTitleData[0]
            };

            _propertyMappingCollection.Apply(series, Arg.Any<MetadataResult<Series>>())
                .Returns(new MetadataResult<Series>
                {
                    HasMetadata = false,
                    Item = new Series
                    {
                        Name = ""
                    }
                });

            var metadataFactory = new AniDbSeriesMetadataFactory(_pluginConfiguration);

            metadataFactory.CreateMetadata(series, "en").ShouldBeEquivalentTo(metadataFactory.NullResult);
        }
    }
}