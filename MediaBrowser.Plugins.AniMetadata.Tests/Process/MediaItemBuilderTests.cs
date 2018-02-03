using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using MediaBrowser.Plugins.AniMetadata.Process;
using NSubstitute;
using NUnit.Framework;
using static LanguageExt.Prelude;

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
            public static EmbyItemData FileEmbyItemData()
            {
                return new EmbyItemData(ItemType.Series, new ItemIdentifier(1, 2, "name"), null, "en");
            }

            public static EmbyItemData LibraryEmbyItemData()
            {
                return new EmbyItemData(ItemType.Series, new ItemIdentifier(1, 2, "name"),
                    new Dictionary<string, int> { { "Key", 1 } }, "en");
            }

            public static ISource Source(string name)
            {
                var source = Substitute.For<ISource>();
                var sourceData = SourceData(source);

                source.Name.Returns(name);
                source.LookupAsync(Arg.Any<IMediaItem>())
                    .ReturnsForAnyArgs(Right<ProcessFailedResult, ISourceData>(sourceData));

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
            public async Task FileData_CreatesMediaItemBasedOnFileSourceData()
            {
                var data = Data.FileEmbyItemData();
                var sourceData = Substitute.For<ISourceData>();

                var fileStructureSource = Substitute.For<ISource>();
                fileStructureSource.LookupAsync(data).Returns(Right<ProcessFailedResult, ISourceData>(sourceData));

                PluginConfiguration.FileStructureSource.Returns(fileStructureSource);

                var result = await Builder.IdentifyAsync(data, ItemType.Series);

                result.IsRight.Should().BeTrue();
                fileStructureSource.Received(1).LookupAsync(data);
            }

            [Test]
            public async Task LibraryData_CreatesMediaItemBasedOnLibrarySourceData()
            {
                var data = Data.LibraryEmbyItemData();
                var sourceData = Substitute.For<ISourceData>();

                var libraryStructureSource = Substitute.For<ISource>();
                libraryStructureSource.LookupAsync(data).Returns(Right<ProcessFailedResult, ISourceData>(sourceData));

                PluginConfiguration.LibraryStructureSource.Returns(libraryStructureSource);

                var result = await Builder.IdentifyAsync(data, ItemType.Series);

                result.IsRight.Should().BeTrue();
                libraryStructureSource.Received(1).LookupAsync(data);
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
            public async Task CallsEverySourceThatDoesNotAlreadyHaveData()
            {
                var existingSource = Substitute.For<ISource>();
                var newSources = _sources.ToList();

                _sources.Add(existingSource);
                _mediaItem.GetDataFromSource(existingSource)
                    .Returns(Option<ISourceData>.Some(Substitute.For<ISourceData>()));
                _mediaItem.AddData(Arg.Any<ISourceData>()).Returns(Right<ProcessFailedResult, IMediaItem>(_mediaItem));

                await Builder.BuildMediaItemAsync(_mediaItem);

                newSources.Iter(s => s.Received(1).LookupAsync(_mediaItem));
                existingSource.DidNotReceive().LookupAsync(_mediaItem);
            }

            [Test]
            public async Task CombinesOutputFromAllSources()
            {
                _mediaItem = new MediaItem(Substitute.For<IEmbyItemData>(), ItemType.Series,
                    Substitute.For<ISourceData>());

                var builtMediaItem = await Builder.BuildMediaItemAsync(_mediaItem);

                _sources.Iter(s => builtMediaItem.ValueUnsafe().GetDataFromSource(s).IsSome.Should().BeTrue());
            }

            [Test]
            public async Task CombinesOutputFromSourcesThatDependOnOtherSources()
            {
                _mediaItem = new MediaItem(Substitute.For<IEmbyItemData>(), ItemType.Series,
                    Substitute.For<ISourceData>());

                var dependentSource = Substitute.For<ISource>();
                var dependentSourceData = Data.SourceData(dependentSource);
                dependentSource.Name.Returns("Dependent");
                dependentSource.LookupAsync(Arg.Any<IMediaItem>())
                    .Returns(x =>
                        x.Arg<IMediaItem>().GetDataFromSource(_sources[1]).IsSome
                            ? Right<ProcessFailedResult, ISourceData>(dependentSourceData)
                            : Left<ProcessFailedResult, ISourceData>(new ProcessFailedResult("Source", "MediaItemName",
                                ItemType.Series, "No parent source data")));

                _sources.Insert(0, dependentSource);

                var builtMediaItem = await Builder.BuildMediaItemAsync(_mediaItem);

                builtMediaItem.IsRight.Should().BeTrue();
                builtMediaItem.ValueUnsafe().GetDataFromSource(dependentSource).IsSome.Should().BeTrue();
            }
        }
    }
}