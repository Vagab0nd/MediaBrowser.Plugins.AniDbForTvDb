using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.PropertyMapping;
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

            PluginConfiguration = Substitute.For<IPluginConfiguration>();

            var embyItemData = Substitute.For<IEmbyItemData>();
            embyItemData.Language.Returns("en");

            MediaItem = Substitute.For<IMediaItem>();
            MediaItem.GetAllSourceData().Returns(Enumerable.Empty<ISourceData>());
            MediaItem.EmbyData.Returns(embyItemData);

            MediaItemType = new MediaItemType<Series>(MediaItemTypeValue.Series, (c, l) => PropertyMappings);
        }

        internal IPluginConfiguration PluginConfiguration;
        internal IMediaItem MediaItem;
        internal MediaItemType<Series> MediaItemType;
        internal IPropertyMappingCollection PropertyMappings;

        [TestFixture]
        internal class CreateMetadataFoundResult : MediaItemTypeTests
        {
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
                PropertyMappings.Apply(Arg.Any<IEnumerable<ISourceData>>(), Arg.Any<MetadataResult<Series>>(),
                        Arg.Any<Action<string>>())
                    .Returns(m => new MetadataResult<Series>
                    {
                        Item = new Series
                        {
                            Name = name
                        }
                    });

                var result = MediaItemType.CreateMetadataFoundResult(PluginConfiguration, MediaItem);

                result.IsLeft.Should().BeTrue();
                result.IfLeft(r => r.Reason.Should().Be("Property mapping failed for the Name property"));
            }

            [Test]
            public void AddsProviderIds()
            {
                var source = Substitute.For<ISource>();
                source.Name.Returns("SourceName");

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
            public void RemovesIdsThatNoLongerExist()
            {
                var source = Substitute.For<ISource>();
                source.Name.Returns("SourceName");

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