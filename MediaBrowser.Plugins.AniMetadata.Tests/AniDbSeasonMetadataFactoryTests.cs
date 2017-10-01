using System;
using System.Collections.Generic;
using System.Linq;
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
    public class AniDbSeasonMetadataFactoryTests
    {
        [SetUp]
        public void Setup()
        {
            _pluginConfiguration = Substitute.For<IPluginConfiguration>();
            _seasonMappings = Substitute.For<IPropertyMappingCollection>();

            _seasonMappings.Apply(Arg.Any<object>(), Arg.Any<MetadataResult<Season>>())
                .Returns(c => new MetadataResult<Season> { Item = new Season() });

            _pluginConfiguration.GetSeasonMetadataMapping().Returns(c => _seasonMappings);
        }

        private IPluginConfiguration _pluginConfiguration;
        private IPropertyMappingCollection _seasonMappings;

        [Test]
        public void CreateMetadata_AppliesMappings()
        {
            var series = new AniDbSeriesData
            {
                Titles = new ItemTitleData[0],
                Description = "Description",
                StartDate = new DateTime(1, 2, 3, 4, 5, 6),
                EndDate = new DateTime(6, 5, 4, 3, 2, 1),
                Ratings = new RatingData[] { new PermanentRatingData { Value = 55.24f } }
            };

            var metadataFactory = new AniDbSeasonMetadataFactory(_pluginConfiguration);

            metadataFactory.CreateMetadata(series, 1);

            _seasonMappings.Received(1).Apply(series, Arg.Is<MetadataResult<Season>>(r => r.Item != null));
        }

        [Test]
        public void CreateMetadata_CombinedData_AppliesMappings()
        {
            var aniDbSeriesData = new AniDbSeriesData
            {
                Titles = new ItemTitleData[0],
                Description = "Description",
                StartDate = new DateTime(1, 2, 3, 4, 5, 6),
                EndDate = new DateTime(6, 5, 4, 3, 2, 1),
                Ratings = new RatingData[] { new PermanentRatingData { Value = 55.24f } }
            };

            var tvDbSeriesData = new TvDbSeriesData(1, "", null, "", 0, DayOfWeek.Friday, "", 1, new List<string>(),
                new List<string>(), "");

            var metadataFactory = new AniDbSeasonMetadataFactory(_pluginConfiguration);

            metadataFactory.CreateMetadata(aniDbSeriesData, tvDbSeriesData, 1);

            _seasonMappings.Received(1)
                .Apply(
                    Arg.Is<IEnumerable<object>>(e => e.SequenceEqual(new object[] { aniDbSeriesData, tvDbSeriesData })),
                    Arg.Is<MetadataResult<Season>>(r => r.Item != null));
        }

        [Test]
        public void CreateMetadata_CombinedData_NameSet_ReturnsMappedMetadata()
        {
            var aniDbSeriesData = new AniDbSeriesData
            {
                Titles = new ItemTitleData[0],
                Description = "Description",
                StartDate = new DateTime(1, 2, 3, 4, 5, 6),
                EndDate = new DateTime(6, 5, 4, 3, 2, 1),
                Ratings = new RatingData[] { new PermanentRatingData { Value = 55.24f } }
            };

            var tvDbSeriesData = new TvDbSeriesData(1, "", null, "", 0, DayOfWeek.Friday, "", 1, new List<string>(),
                new List<string>(), "");

            var mappedMetadata = new MetadataResult<Season> { Item = new Season { Name = "Name" } };

            _seasonMappings
                .Apply(
                    Arg.Is<IEnumerable<object>>(e => e.SequenceEqual(new object[] { aniDbSeriesData, tvDbSeriesData })),
                    Arg.Any<MetadataResult<Season>>())
                .Returns(mappedMetadata);

            var metadataFactory = new AniDbSeasonMetadataFactory(_pluginConfiguration);

            metadataFactory.CreateMetadata(aniDbSeriesData, tvDbSeriesData, 1).Should().BeSameAs(mappedMetadata);
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(0)]
        public void CreateMetadata_CombinedData_NameSet_SetsIndex(int seasonIndex)
        {
            var aniDbSeriesData = new AniDbSeriesData
            {
                Titles = new ItemTitleData[0],
                Description = "Description",
                StartDate = new DateTime(1, 2, 3, 4, 5, 6),
                EndDate = new DateTime(6, 5, 4, 3, 2, 1),
                Ratings = new RatingData[] { new PermanentRatingData { Value = 55.24f } }
            };

            var tvDbSeriesData = new TvDbSeriesData(1, "", null, "", 0, DayOfWeek.Friday, "", 1, new List<string>(),
                new List<string>(), "");

            var mappedMetadata = new MetadataResult<Season> { Item = new Season { Name = "Name" } };

            _seasonMappings
                .Apply(
                    Arg.Is<IEnumerable<object>>(e => e.SequenceEqual(new object[] { aniDbSeriesData, tvDbSeriesData })),
                    Arg.Any<MetadataResult<Season>>())
                .Returns(mappedMetadata);

            var metadataFactory = new AniDbSeasonMetadataFactory(_pluginConfiguration);

            metadataFactory.CreateMetadata(aniDbSeriesData, tvDbSeriesData, seasonIndex)
                .Item.IndexNumber.Should()
                .Be(seasonIndex);
        }

        [Test]
        public void CreateMetadata_CombinedData_NoNameSet_ReturnsNullResult()
        {
            var aniDbSeriesData = new AniDbSeriesData
            {
                Titles = new ItemTitleData[0],
                Description = "Description",
                StartDate = new DateTime(1, 2, 3, 4, 5, 6),
                EndDate = new DateTime(6, 5, 4, 3, 2, 1),
                Ratings = new RatingData[] { new PermanentRatingData { Value = 55.24f } }
            };

            var tvDbSeriesData = new TvDbSeriesData(1, "", null, "", 0, DayOfWeek.Friday, "", 1, new List<string>(),
                new List<string>(), "");

            var metadataFactory = new AniDbSeasonMetadataFactory(_pluginConfiguration);

            metadataFactory.CreateMetadata(aniDbSeriesData, tvDbSeriesData, 1)
                .ShouldBeEquivalentTo(metadataFactory.NullResult);
        }

        [Test]
        public void CreateMetadata_NameSet_ReturnsMappedMetadata()
        {
            var series = new AniDbSeriesData
            {
                Titles = new ItemTitleData[0]
            };

            var mappedMetadata = new MetadataResult<Season> { Item = new Season { Name = "Name" } };

            _seasonMappings.Apply(series, Arg.Any<MetadataResult<Season>>()).Returns(mappedMetadata);

            var metadataFactory = new AniDbSeasonMetadataFactory(_pluginConfiguration);

            metadataFactory.CreateMetadata(series, 1).Should().BeSameAs(mappedMetadata);
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(0)]
        public void CreateMetadata_NameSet_SetsIndex(int seasonIndex)
        {
            var series = new AniDbSeriesData
            {
                Titles = new ItemTitleData[0]
            };

            var mappedMetadata = new MetadataResult<Season> { Item = new Season { Name = "Name" } };

            _seasonMappings.Apply(series, Arg.Any<MetadataResult<Season>>()).Returns(mappedMetadata);

            var metadataFactory = new AniDbSeasonMetadataFactory(_pluginConfiguration);

            metadataFactory.CreateMetadata(series, seasonIndex).Item.IndexNumber.Should().Be(seasonIndex);
        }

        [Test]
        public void CreateMetadata_NoNameSet_ReturnsNullResult()
        {
            var series = new AniDbSeriesData
            {
                Titles = new ItemTitleData[0]
            };

            var metadataFactory = new AniDbSeasonMetadataFactory(_pluginConfiguration);

            metadataFactory.CreateMetadata(series, 1).ShouldBeEquivalentTo(metadataFactory.NullResult);
        }
    }
}