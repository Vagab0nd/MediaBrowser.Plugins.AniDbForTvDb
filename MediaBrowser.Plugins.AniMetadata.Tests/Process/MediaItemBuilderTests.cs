using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.Process;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests.Process
{
    [TestFixture]
    public class MediaItemBuilderTests
    {
        [SetUp]
        public virtual void Setup()
        {
            PluginConfiguration = Substitute.For<INewPluginConfiguration>();

            Builder = new MediaItemBuilder(PluginConfiguration, null);
        }

        internal INewPluginConfiguration PluginConfiguration;
        internal MediaItemBuilder Builder;

        internal static class Data
        {
            public static IMediaItem MediaItem()
            {
                return Substitute.For<IMediaItem>();
            }

            public static EmbyItemData FileEmbyItemData()
            {
                return new EmbyItemData(new ItemIdentifier(1, 2, "name"), null);
            }

            public static EmbyItemData LibraryEmbyItemData()
            {
                return new EmbyItemData(new ItemIdentifier(1, 2, "name"),
                    new Dictionary<string, int> { { "Key", 1 } });
            }

            public static ISource Source(string name)
            {
                var source = Substitute.For<ISource>();
                var sourceData = SourceData(source);

                source.Name.Returns(name);
                source.Lookup(Arg.Any<IMediaItem>())
                    .ReturnsForAnyArgs(Option<ISourceData>.Some(sourceData));

                return source;
            }

            public static ISourceData SourceData(ISource source)
            {
                var sourceData = Substitute.For<ISourceData>();

                sourceData.Source.Returns(source);

                return sourceData;
            }
        }

        public class Identify : MediaItemBuilderTests
        {
            [Test]
            public void FileData_CreatesMediaItemBasedOnFileSourceData()
            {
                var data = Data.FileEmbyItemData();
                var sourceData = Substitute.For<ISourceData>();

                var fileStructureSource = Substitute.For<ISource>();
                fileStructureSource.Lookup(data).Returns(Option<ISourceData>.Some(sourceData));

                PluginConfiguration.FileStructureSource.Returns(fileStructureSource);

                Builder.Identify(data, ItemType.Series).IsSome.Should().BeTrue();

                fileStructureSource.Received(1).Lookup(data);
            }

            [Test]
            public void LibraryData_CreatesMediaItemBasedOnLibrarySourceData()
            {
                var data = Data.LibraryEmbyItemData();
                var sourceData = Substitute.For<ISourceData>();

                var libraryStructureSource = Substitute.For<ISource>();
                libraryStructureSource.Lookup(data).Returns(Option<ISourceData>.Some(sourceData));

                PluginConfiguration.LibraryStructureSource.Returns(libraryStructureSource);

                Builder.Identify(data, ItemType.Series).IsSome.Should().BeTrue();

                libraryStructureSource.Received(1).Lookup(data);
            }
        }

        [TestFixture]
        public class BuildMediaItem : MediaItemBuilderTests
        {
            [SetUp]
            public override void Setup()
            {
                _sources = new[]
                {
                    Data.Source("A"),
                    Data.Source("B"),
                    Data.Source("C")
                }.ToList();
                
                Builder = new MediaItemBuilder(PluginConfiguration, _sources);

                _mediaItem = Substitute.For<IMediaItem>();
            }

            private IList<ISource> _sources;
            private IMediaItem _mediaItem;

            [Test]
            public void CallsEverySourceThatDoesNotAlreadyHaveData()
            {
                var existingSource = Substitute.For<ISource>();
                var newSources = _sources.ToList();

                _sources.Add(existingSource);
                _mediaItem.GetDataFromSource(existingSource)
                    .Returns(Option<ISourceData>.Some(Substitute.For<ISourceData>()));
                _mediaItem.AddData(Arg.Any<ISourceData>()).Returns(_mediaItem);

                Builder.BuildMediaItem(_mediaItem);

                newSources.Iter(s => s.Received(1).Lookup(_mediaItem));
                existingSource.DidNotReceive().Lookup(_mediaItem);
            }

            [Test]
            public void CombinesOutputFromAllSources()
            {
                _mediaItem = new MediaItem(ItemType.Series, Substitute.For<ISourceData>());

                var builtMediaItem = Builder.BuildMediaItem(_mediaItem);

                _sources.Iter(s => builtMediaItem.GetDataFromSource(s).IsSome.Should().BeTrue());
            }

            [Test]
            public void CombinesOutputFromSourcesThatDependOnOtherSources()
            {
                _mediaItem = new MediaItem(ItemType.Series, Substitute.For<ISourceData>());

                var dependentSource = Substitute.For<ISource>();
                var dependentSourceData = Data.SourceData(dependentSource);
                dependentSource.Name.Returns("Dependent");
                dependentSource.Lookup(Arg.Any<IMediaItem>())
                    .Returns(x =>
                        x.Arg<IMediaItem>().GetDataFromSource(_sources[1]).IsSome
                            ? Option<ISourceData>.Some(dependentSourceData)
                            : Option<ISourceData>.None);

                _sources.Insert(0, dependentSource);

                var builtMediaItem = Builder.BuildMediaItem(_mediaItem);

                builtMediaItem.GetDataFromSource(dependentSource).IsSome.Should().BeTrue();
            }
        }
    }
}