using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LanguageExt.UnsafeValueAccess;
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
                .Returns(new MetadataResult<Series>
                {
                    Item = new Series()
                });

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
            public void NameIsMapped_SetsHasMetadataToTrue()
            {
                PropertyMappings.Apply(Arg.Any<IEnumerable<object>>(), Arg.Any<MetadataResult<Series>>(),
                        Arg.Any<Action<string>>())
                    .Returns(m => new MetadataResult<Series>
                    {
                        Item = new Series
                        {
                            Name = "Name"
                        }
                    });

                var result = MediaItemType.CreateMetadataFoundResult(PluginConfiguration, MediaItem);

                result.IsRight.Should().BeTrue();
                result.IfRight(r => r.GetResult<Series>().ValueUnsafe().HasMetadata.Should().BeTrue());
            }

            [Test]
            [TestCase("")]
            [TestCase(null)]
            [TestCase("   ")]
            public void NameNotMapped_SetsHasMetadataToFalse(string name)
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

                result.IsRight.Should().BeTrue();
                result.IfRight(r => r.GetResult<Series>().ValueUnsafe().HasMetadata.Should().BeFalse());
            }
        }
    }
}