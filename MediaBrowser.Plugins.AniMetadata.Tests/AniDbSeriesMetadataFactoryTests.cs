using System;
using System.Collections.Generic;
using FluentAssertions;
using LanguageExt;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.PropertyMapping;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
using MediaBrowser.Plugins.AniMetadata.TvDb;
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

            _log = new ConsoleLogManager();
        }

        private IPropertyMappingCollection _propertyMappingCollection;
        private IPluginConfiguration _pluginConfiguration;
        private ILogManager _log;

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
                .Apply(series, Arg.Is<MetadataResult<Series>>(r => r.HasMetadata && r.Item != null),
                    Arg.Any<Action<string>>())
                .Returns(expectedResult);

            var metadataFactory = new AniDbSeriesMetadataFactory(_pluginConfiguration, _log);

            metadataFactory.CreateMetadata(series, "en").Should().Be(expectedResult);
        }

        [Test]
        public void CreateMetadata_MultipleSources_HasTitle_ReturnsPopulatedSeries()
        {
            var aniDbSeries = new AniDbSeriesData
            {
                Id = 44
            };
            var tvDbSeries = new TvDbSeriesData(66, "", Option<DateTime>.None, "", 0, AirDay.Monday, "", 0, new List<string>(),
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
                    Arg.Any<MetadataResult<Series>>(), Arg.Any<Action<string>>())
                .Returns(expectedResult);

            var metadataFactory = new AniDbSeriesMetadataFactory(_pluginConfiguration, _log);

            metadataFactory.CreateMetadata(aniDbSeries, tvDbSeries, "en").Should().Be(expectedResult);
        }

        [Test]
        public void CreateMetadata_MultipleSources_NoTitle_ReturnsNullResult()
        {
            var aniDbSeries = new AniDbSeriesData
            {
                Id = 44
            };
            var tvDbSeries = new TvDbSeriesData(66, "", Option<DateTime>.None, "", 0, AirDay.Monday, "", 0, new List<string>(),
                new List<string>(), "");

            _propertyMappingCollection.Apply(Arg.Is<object[]>(a => a[0] == aniDbSeries && a[1] == tvDbSeries),
                    Arg.Any<MetadataResult<Series>>(), Arg.Any<Action<string>>())
                .Returns(new MetadataResult<Series>
                {
                    HasMetadata = false,
                    Item = new Series
                    {
                        Name = ""
                    }
                });

            var metadataFactory = new AniDbSeriesMetadataFactory(_pluginConfiguration, _log);

            metadataFactory.CreateMetadata(aniDbSeries, tvDbSeries, "en")
                .Should().BeEquivalentTo(metadataFactory.NullResult);
        }

        [Test]
        public void CreateMetadata_NoTitle_ReturnsNullResult()
        {
            var series = new AniDbSeriesData
            {
                Titles = new ItemTitleData[0]
            };

            _propertyMappingCollection.Apply(series, Arg.Any<MetadataResult<Series>>(), Arg.Any<Action<string>>())
                .Returns(new MetadataResult<Series>
                {
                    HasMetadata = false,
                    Item = new Series
                    {
                        Name = ""
                    }
                });

            var metadataFactory = new AniDbSeriesMetadataFactory(_pluginConfiguration, _log);

            metadataFactory.CreateMetadata(series, "en").Should().BeEquivalentTo(metadataFactory.NullResult);
        }
    }
}