using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.Process;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests.Process
{
    [TestFixture]
    public class MediaItemProcessorTests
    {
        [SetUp]
        public virtual void Setup()
        {
            EmbyResultFactory = Substitute.For<IEmbyResultFactory>();
            MediaItemBuilder = Substitute.For<IMediaItemBuilder>();

            Processor = new MediaItemProcessor(MediaItemBuilder, EmbyResultFactory);
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

        internal IEmbyResultFactory EmbyResultFactory;
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
                    .Returns(OptionAsync<IMediaItem>.Some(mediaItem));

                var result = await Processor.GetResultAsync(embyInfo, ItemType.Series).ToOption();

                result.IsSome.Should().BeTrue();
                MediaItemBuilder.Received(1).BuildMediaItemAsync(mediaItem);
            }

            [Test]
            public async Task CreatesResult()
            {
                var embyInfo = Data.EmbyInfo();
                var mediaItem = Data.MediaItem();
                var builtMediaItem = Data.MediaItem();

                MediaItemBuilder.IdentifyAsync(Arg.Any<EmbyItemData>(), ItemType.Series)
                    .Returns(OptionAsync<IMediaItem>.Some(mediaItem));
                MediaItemBuilder.BuildMediaItemAsync(mediaItem).Returns(builtMediaItem);

                var result = await Processor.GetResultAsync(embyInfo, ItemType.Series).ToOption();

                result.IsSome.Should().BeTrue();
                EmbyResultFactory.Received(1).GetResult(builtMediaItem);
            }

            [Test]
            public async Task IdentifiesItem()
            {
                var embyInfo = Data.EmbyInfo();

                var result = await Processor.GetResultAsync(embyInfo, ItemType.Series).ToOption();

                result.IsSome.Should().BeFalse();

                MediaItemBuilder.Received(1)
                    .IdentifyAsync(Arg.Is<EmbyItemData>(d => d.Identifier.Index == 1 &&
                        d.Identifier.ParentIndex == 2 &&
                        d.Identifier.Name == "name"), ItemType.Series);
            }
        }
    }
}