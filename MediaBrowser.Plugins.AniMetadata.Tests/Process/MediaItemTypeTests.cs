using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LanguageExt;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using MediaBrowser.Plugins.AniMetadata.PropertyMapping;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests.Process
{
    [TestFixture]
    internal class MediaItemTypeTests
    {
        [SetUp]
        public void Setup()
        {
            PropertyMappings = Substitute.For<IPropertyMappingCollection>();
            PropertyMappings.Apply(Arg.Any<IEnumerable<object>>(), Arg.Any<MetadataResult<Series>>(),
                    Arg.Any<Action<string>>())
                .Returns(x => x.Arg<MetadataResult<Series>>());
            PropertyMappings.GetEnumerator()
                .Returns(new IPropertyMapping[]
                {
                    new PropertyMapping<AniDbSeriesData, MetadataResult<Series>, string>("Name", s => s.Item.Name,
                        (s, t) => t.Item.Name = "Name", "AniDb")
                }.AsEnumerable().GetEnumerator());

            PluginConfiguration = Substitute.For<IPluginConfiguration>();

            var embyItemData = Substitute.For<IEmbyItemData>();
            embyItemData.Language.Returns("en");

            var sources = new TestSources();
            var aniDbSourceData = new SourceData<AniDbSeriesData>(sources.AniDb, 33, new ItemIdentifier(33, 1, "Name"),
                new AniDbSeriesData());

            MediaItem = Substitute.For<IMediaItem>();
            MediaItem.GetAllSourceData().Returns(new ISourceData[] { aniDbSourceData });
            MediaItem.GetDataFromSource(null).ReturnsForAnyArgs(aniDbSourceData);
            MediaItem.EmbyData.Returns(embyItemData);

            MediaItemType = new MediaItemType<Series>((c, l) => PropertyMappings);
        }

        internal IPluginConfiguration PluginConfiguration;
        internal IMediaItem MediaItem;
        internal MediaItemType<Series> MediaItemType;
        internal IPropertyMappingCollection PropertyMappings;

        [TestFixture]
        internal class CreateMetadataFoundResult : MediaItemTypeTests
        {
            [Test]
            public void AddsProviderIds()
            {
                var source = Substitute.For<ISource>();
                source.Name.Returns(new SourceName("SourceName"));

                var sourceData = Substitute.For<ISourceData>();
                sourceData.Id.Returns(3);
                sourceData.Source.Returns(source);

                MediaItem.GetAllSourceData().Returns(new[] { sourceData });

                PropertyMappings.Apply(Arg.Any<IEnumerable<object>>(), Arg.Any<MetadataResult<Series>>(),
                        Arg.Any<Action<string>>())
                    .Returns(m =>
                    {
                        var r = m.Arg<MetadataResult<Series>>();

                        r.Item.Name = "Name";

                        return r;
                    });

                var result = MediaItemType.CreateMetadataFoundResult(PluginConfiguration, MediaItem);

                result.IsRight.Should().BeTrue();
                result.IfRight(r =>
                {
                    r.EmbyMetadataResult.Item.ProviderIds.Should().ContainKey("SourceName");
                    r.EmbyMetadataResult.Item.ProviderIds.Should().ContainValue("3");
                });
            }

            [Test]
            public void AppliesPropertyMappings()
            {
                MediaItemType.CreateMetadataFoundResult(PluginConfiguration, MediaItem);

                PropertyMappings.Received(1)
                    .Apply(Arg.Any<IEnumerable<object>>(), Arg.Any<MetadataResult<Series>>(),
                        Arg.Any<Action<string>>());
            }

            [Test]
            public void NameIsMapped_ReturnsResultWithHasMetadataToTrue()
            {
                PropertyMappings.Apply(Arg.Any<IEnumerable<object>>(), Arg.Any<MetadataResult<Series>>(),
                        Arg.Any<Action<string>>())
                    .Returns(m =>
                    {
                        var r = m.Arg<MetadataResult<Series>>();

                        r.Item.Name = "Name";

                        return r;
                    });

                var result = MediaItemType.CreateMetadataFoundResult(PluginConfiguration, MediaItem);

                result.IsRight.Should().BeTrue();
                result.IfRight(r => r.EmbyMetadataResult.HasMetadata.Should().BeTrue());
            }

            [Test]
            [TestCase("")]
            [TestCase(null)]
            [TestCase("   ")]
            public void NameNotMapped_ReturnsFailure(string name)
            {
                PropertyMappings.GetEnumerator()
                .Returns(new IPropertyMapping[]
                {
                    new PropertyMapping<AniDbSeriesData, MetadataResult<Series>, string>("Name", s => s.Item.Name,
                        (s, t) => t.Item.Name = name, "AniDb")
                }.AsEnumerable().GetEnumerator());
                
                var result = MediaItemType.CreateMetadataFoundResult(PluginConfiguration, MediaItem);

                result.IsLeft.Should().BeTrue();
                result.IfLeft(r => r.Reason.Should().Be("Property mapping failed for the Name property"));
            }

            [Test]
            public void NoLibrarySourceData_ReturnsFailure()
            {
                MediaItem.GetAllSourceData().Returns(new ISourceData[] { });
                MediaItem.GetDataFromSource(null).ReturnsForAnyArgs(Option<ISourceData>.None);

                var result = MediaItemType.CreateMetadataFoundResult(PluginConfiguration, MediaItem);

                result.IsLeft.Should().BeTrue();
                result.IfLeft(r => r.Reason.Should().Be("No data returned by library structure source"));
            }

            [Test]
            public void NoNameMappingForLibrarySourceData_ReturnsFailure()
            {
                PropertyMappings.GetEnumerator()
                    .Returns(Enumerable.Empty<IPropertyMapping>().GetEnumerator());

                var result = MediaItemType.CreateMetadataFoundResult(PluginConfiguration, MediaItem);

                result.IsLeft.Should().BeTrue();
                result.IfLeft(r => r.Reason.Should().Be("No value for Name property mapped from library source"));
            }

            [Test]
            public void RemovesIdsThatNoLongerExist()
            {
                var source = Substitute.For<ISource>();
                source.Name.Returns(new SourceName("SourceName"));

                var sourceData = Substitute.For<ISourceData>();
                sourceData.Source.Returns(source);

                MediaItem.GetAllSourceData().Returns(new[] { sourceData });

                PropertyMappings.Apply(Arg.Any<IEnumerable<object>>(), Arg.Any<MetadataResult<Series>>(),
                        Arg.Any<Action<string>>())
                    .Returns(m =>
                    {
                        var r = m.Arg<MetadataResult<Series>>();

                        r.Item.Name = "Name";
                        r.Item.ProviderIds = new Dictionary<string, string> { { "SourceName", "3" } };

                        return r;
                    });

                var result = MediaItemType.CreateMetadataFoundResult(PluginConfiguration, MediaItem);

                result.IsRight.Should().BeTrue();
                result.IfRight(r =>
                {
                    r.EmbyMetadataResult.Item.ProviderIds.Should().NotContainKey("SourceName");
                    r.EmbyMetadataResult.Item.ProviderIds.Should().NotContainValue("3");
                });
            }
        }
    }
}