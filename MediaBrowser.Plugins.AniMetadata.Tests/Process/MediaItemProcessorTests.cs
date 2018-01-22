using System.Collections.Generic;
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
                return new ItemLookupInfo()
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
            public void BuildsMediaItem()
            {
                var embyInfo = Data.EmbyInfo();
                var mediaItem = Data.MediaItem();

                MediaItemBuilder.Identify(Arg.Any<EmbyItemData>(), ItemType.Series)
                    .Returns(Option<IMediaItem>.Some(mediaItem));

                Processor.GetResult(embyInfo, ItemType.Series).IsSome.Should().BeTrue();

                MediaItemBuilder.Received(1).BuildMediaItem(mediaItem);
            }

            [Test]
            public void CreatesResult()
            {
                var embyInfo = Data.EmbyInfo();
                var mediaItem = Data.MediaItem();
                var builtMediaItem = Data.MediaItem();

                MediaItemBuilder.Identify(Arg.Any<EmbyItemData>(), ItemType.Series)
                    .Returns(Option<IMediaItem>.Some(mediaItem));
                MediaItemBuilder.BuildMediaItem(mediaItem).Returns(builtMediaItem);

                Processor.GetResult(embyInfo, ItemType.Series).IsSome.Should().BeTrue();

                EmbyResultFactory.Received(1).GetResult(builtMediaItem);
            }

            [Test]
            public void IdentifiesItem()
            {
                var embyInfo = Data.EmbyInfo();

                Processor.GetResult(embyInfo, ItemType.Series).IsSome.Should().BeFalse();

                MediaItemBuilder.Received(1).Identify(Arg.Is<EmbyItemData>(d => d.Identifier.Index == 1 &&
                    d.Identifier.ParentIndex == 2 &&
                    d.Identifier.Name == "name"), ItemType.Series);
            }
        }
    }
}