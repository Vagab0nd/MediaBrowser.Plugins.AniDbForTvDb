using System.Threading.Tasks;
using FluentAssertions;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.Process;
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
            ResultFactory = Substitute.For<IResultFactory>();
            MediaItemBuilder = Substitute.For<IMediaItemBuilder>();

            MediaItemBuilder.BuildMediaItemAsync(Arg.Any<IMediaItem>())
                .Returns(x => Right<ProcessFailedResult, IMediaItem>(x.Arg<IMediaItem>()));
            ResultFactory.GetResult(Arg.Any<IMediaItem>())
                .Returns(Right<ProcessFailedResult, IMetadataFoundResult>(Substitute.For<IMetadataFoundResult>()));

            Processor = new MediaItemProcessor(MediaItemBuilder, ResultFactory);
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

        internal IResultFactory ResultFactory;
        internal IMediaItemBuilder MediaItemBuilder;
        internal MediaItemProcessor Processor;

        [TestFixture]
        public class GetResult : MediaItemProcessorTests
        {
            [Test]
            public async Task BuildsMediaItem()
            {
                var embyInfo = Data.EmbyInfo();
                var mediaItem = Data.MediaItem();

                MediaItemBuilder.IdentifyAsync(Arg.Any<EmbyItemData>(), ItemType.Series)
                    .Returns(Right<ProcessFailedResult, IMediaItem>(mediaItem));

                var result = await Processor.GetResultAsync(embyInfo, ItemType.Series);

                result.IsRight.Should().BeTrue();
                MediaItemBuilder.Received(1).BuildMediaItemAsync(mediaItem);
            }

            [Test]
            public async Task CreatesResult()
            {
                var embyInfo = Data.EmbyInfo();
                var mediaItem = Data.MediaItem();
                var builtMediaItem = Data.MediaItem();

                MediaItemBuilder.IdentifyAsync(Arg.Any<EmbyItemData>(), ItemType.Series)
                    .Returns(Right<ProcessFailedResult, IMediaItem>(mediaItem));
                MediaItemBuilder.BuildMediaItemAsync(mediaItem)
                    .Returns(Right<ProcessFailedResult, IMediaItem>(builtMediaItem));

                var result = await Processor.GetResultAsync(embyInfo, ItemType.Series);

                result.IsRight.Should().BeTrue();
                ResultFactory.Received(1).GetResult(builtMediaItem);
            }

            [Test]
            public async Task IdentifiesItem()
            {
                var embyInfo = Data.EmbyInfo();
                var mediaItem = Data.MediaItem();

                MediaItemBuilder.IdentifyAsync(Arg.Any<EmbyItemData>(), ItemType.Series)
                    .Returns(Right<ProcessFailedResult, IMediaItem>(mediaItem));

                var result = await Processor.GetResultAsync(embyInfo, ItemType.Series);

                result.IsRight.Should().BeTrue();

                MediaItemBuilder.Received(1)
                    .IdentifyAsync(Arg.Is<EmbyItemData>(d => d.Identifier.Index == 1 &&
                        d.Identifier.ParentIndex == 2 &&
                        d.Identifier.Name == "name"), ItemType.Series);
            }
        }
    }
}