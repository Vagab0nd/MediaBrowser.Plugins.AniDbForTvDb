using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
using NSubstitute;
using NUnit.Framework;
using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata.Tests.Process
{
    [TestFixture]
    public class MediaItemProcessorTests
    {
        [SetUp]
        public virtual void Setup()
        {
            PluginConfiguration = Substitute.For<IPluginConfiguration>();

            MediaItemBuilder = Substitute.For<IMediaItemBuilder>();
            MediaItemBuilder.BuildMediaItemAsync(Arg.Any<IMediaItem>())
                .Returns(x => Right<ProcessFailedResult, IMediaItem>(x.Arg<IMediaItem>()));

            MediaItemType = Substitute.For<IMediaItemType<Series>>();
            MediaItemType.CreateMetadataFoundResult(PluginConfiguration, Arg.Any<IMediaItem>(), Arg.Any<ILogManager>())
                .Returns(x => Right<ProcessFailedResult, IMetadataFoundResult<Series>>(new MetadataFoundResult<Series>(
                    x.Arg<IMediaItem>(), new MetadataResult<Series>
                    {
                        Item = new Series()
                    })));

            Processor = new MediaItemProcessor(PluginConfiguration, MediaItemBuilder, new ConsoleLogManager());
        }

        internal static class Data
        {
            public static IMediaItem MediaItem()
            {
                return Substitute.For<IMediaItem>();
            }

            public static ItemLookupInfo EmbyInfo()
            {
                return new ItemLookupInfo
                {
                    IndexNumber = 1,
                    ParentIndexNumber = 2,
                    Name = "name"
                };
            }
        }

        internal IMediaItemType<Series> MediaItemType;
        internal IPluginConfiguration PluginConfiguration;
        internal IMediaItemBuilder MediaItemBuilder;
        internal MediaItemProcessor Processor;

        [TestFixture]
        public class GetResultAsync : MediaItemProcessorTests
        {
            [SetUp]
            public override void Setup()
            {
                base.Setup();

                this.mediaItem = Data.MediaItem();
                this.embyInfo = Data.EmbyInfo();

                MediaItemBuilder.IdentifyAsync(Arg.Any<EmbyItemData>(), MediaItemType)
                    .Returns(Right<ProcessFailedResult, IMediaItem>(this.mediaItem));
            }

            private ItemLookupInfo embyInfo;
            private IMediaItem mediaItem;

            [Test]
            public async Task BuildsMediaItem()
            {
                MediaItemBuilder.IdentifyAsync(Arg.Any<EmbyItemData>(), MediaItemType)
                    .Returns(Right<ProcessFailedResult, IMediaItem>(this.mediaItem));

                var result = await Processor.GetResultAsync(this.embyInfo, MediaItemType, Enumerable.Empty<EmbyItemId>());

                result.IsRight.Should().BeTrue();
                await this.MediaItemBuilder.Received(1).BuildMediaItemAsync(this.mediaItem);
            }

            [Test]
            public async Task CreatesResult()
            {
                var embyInfo = Data.EmbyInfo();
                var mediaItem = Data.MediaItem();
                var builtMediaItem = Data.MediaItem();

                MediaItemBuilder.IdentifyAsync(Arg.Any<EmbyItemData>(), MediaItemType)
                    .Returns(Right<ProcessFailedResult, IMediaItem>(mediaItem));
                MediaItemBuilder.BuildMediaItemAsync(mediaItem)
                    .Returns(Right<ProcessFailedResult, IMediaItem>(builtMediaItem));

                var result = await Processor.GetResultAsync(embyInfo, MediaItemType, Enumerable.Empty<EmbyItemId>());

                result.IsRight.Should().BeTrue();
                result.IfRight(r => r.MediaItem.Should().Be(builtMediaItem));
            }

            [Test]
            public async Task IdentifiesItem()
            {
                var embyInfo = Data.EmbyInfo();
                var mediaItem = Data.MediaItem();

                MediaItemBuilder.IdentifyAsync(Arg.Any<EmbyItemData>(), MediaItemType)
                    .Returns(Right<ProcessFailedResult, IMediaItem>(mediaItem));

                var result = await Processor.GetResultAsync(embyInfo, MediaItemType, Enumerable.Empty<EmbyItemId>());

                result.IsRight.Should().BeTrue();

                await this.MediaItemBuilder.Received(1)
                    .IdentifyAsync(Arg.Is<EmbyItemData>(d => d.Identifier.Index == 1 &&
                                                             d.Identifier.ParentIndex == 2 &&
                                                             d.Identifier.Name == "name"), this.MediaItemType);
            }
        }
    }
}