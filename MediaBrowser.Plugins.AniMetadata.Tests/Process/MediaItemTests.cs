using System;
using FluentAssertions;
using LanguageExt.UnsafeValueAccess;
using MediaBrowser.Plugins.AniMetadata.Process;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests.Process
{
    [TestFixture]
    public class MediaItemTests
    {
        [SetUp]
        public void Setup()
        {
            Source = Substitute.For<ISource>();
            Source.Name.Returns("Source");
            SourceData = Substitute.For<ISourceData>();

            SourceData.Source.Returns(Source);

            Source2 = Substitute.For<ISource>();
            Source2.Name.Returns("Source2");
            SourceData2 = Substitute.For<ISourceData>();

            SourceData2.Source.Returns(Source2);
        }

        internal ISource Source;
        internal ISourceData SourceData;
        internal ISource Source2;
        internal ISourceData SourceData2;

        [TestFixture]
        public class Constructor : MediaItemTests
        {
            [Test]
            public void AddsInitialData()
            {
                var mediaItem = new MediaItem(ItemType.Series, SourceData);

                mediaItem.GetDataFromSource(Source).ValueUnsafe().Should().Be(SourceData);
            }

            [Test]
            public void InitialisesItemType()
            {
                var mediaItem = new MediaItem(ItemType.Series, SourceData);

                mediaItem.ItemType.Should().Be(ItemType.Series);
            }

            [Test]
            public void NullData_ThrowsArgumentNullException()
            {
                Action action = () => new MediaItem(ItemType.Series, null);

                action.ShouldThrow<ArgumentNullException>();
            }
        }

        [TestFixture]
        public class AddData : MediaItemTests
        {
            [Test]
            public void DoesNotModifyInstanceCalledOn()
            {
                var mediaItem = new MediaItem(ItemType.Series, SourceData);

                mediaItem.AddData(SourceData2);

                mediaItem.GetDataFromSource(Source).ValueUnsafe().Should().Be(SourceData);
                mediaItem.GetDataFromSource(Source2).IsNone.Should().BeTrue();
            }

            [Test]
            public void ExistingDataFromSource_ThrowsInvalidOperationException()
            {
                var mediaItem = new MediaItem(ItemType.Series, SourceData);

                Action action = () => mediaItem.AddData(SourceData);

                action.ShouldThrow<InvalidOperationException>();
            }

            [Test]
            public void ReturnsMediaItemWithSourceDataAdded()
            {
                var mediaItem = new MediaItem(ItemType.Series, SourceData);

                var mediaItem2 = mediaItem.AddData(SourceData2);

                mediaItem2.GetDataFromSource(Source2).ValueUnsafe().Should().Be(SourceData2);
            }
        }

        [TestFixture]
        public class GetDataFromSource : MediaItemTests
        {
            [Test]
            public void NoDataWithMatchingSource_ReturnsNone()
            {
                var mediaItem = new MediaItem(ItemType.Series, SourceData);

                mediaItem.GetDataFromSource(Source2).IsNone.Should().BeTrue();
            }
        }
    }
}