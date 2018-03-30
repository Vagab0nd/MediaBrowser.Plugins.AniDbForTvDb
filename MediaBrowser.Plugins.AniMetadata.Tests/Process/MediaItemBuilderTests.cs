using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using MediaBrowser.Plugins.AniMetadata.SourceDataLoaders;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
using NSubstitute;
using NSubstitute.ClearExtensions;
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
            PluginConfiguration = Substitute.For<IPluginConfiguration>();

            Builder = new MediaItemBuilder(PluginConfiguration, null, new ConsoleLogManager());
        }

        internal IPluginConfiguration PluginConfiguration;
        internal MediaItemBuilder Builder;

        internal static class Data
        {
            public static EmbyItemData FileEmbyItemData()
            {
                return new EmbyItemData(MediaItemTypes.Series, new ItemIdentifier(1, 2, "name"), null, "en",
                    Enumerable.Empty<EmbyItemId>());
            }

            public static EmbyItemData LibraryEmbyItemData()
            {
                return new EmbyItemData(MediaItemTypes.Series, new ItemIdentifier(1, 2, "name"),
                    new Dictionary<string, int> { { "Key", 1 } }, "en", Enumerable.Empty<EmbyItemId>());
            }

            public static ISourceDataLoader SourceDataLoader(IMediaItem mediaItem, ISourceData dependencySourceData,
                string sourceName)
            {
                var sourceDataLoader = Substitute.For<ISourceDataLoader>();
                var producedSourceData = SourceData(sourceName);

                sourceDataLoader.CanLoadFrom(dependencySourceData).Returns(true);
                sourceDataLoader.CanLoadFrom(Arg.Is<object>(o => o != dependencySourceData)).Returns(false);
                sourceDataLoader.LoadFrom(mediaItem, dependencySourceData)
                    .ReturnsForAnyArgs(Right<ProcessFailedResult, ISourceData>(producedSourceData));

                return sourceDataLoader;
            }

            public static ISourceData SourceData(string sourceName)
            {
                var sourceData = Substitute.For<ISourceData>();
                var source = Substitute.For<ISource>();
                source.Name.Returns(new SourceName(sourceName));

                sourceData.Source.Returns(source);

                return sourceData;
            }
        }

        public class Identify : MediaItemBuilderTests
        {
            [Test]
            public async Task FileData_UsesFileSourceLoader()
            {
                var data = Data.FileEmbyItemData();
                var sourceData = Substitute.For<ISourceData>();
                var source = TestSources.AniDbSource;
                sourceData.Source.Returns(source);
                var embySourceDataLoader = Substitute.For<IEmbySourceDataLoader>();
                embySourceDataLoader.LoadFrom(data).Returns(Right<ProcessFailedResult, ISourceData>(sourceData));

                var fileStructureSource = Substitute.For<ISource>();
                fileStructureSource.GetEmbySourceDataLoader(MediaItemTypes.Series)
                    .Returns(Right<ProcessFailedResult, IEmbySourceDataLoader>(embySourceDataLoader));

                PluginConfiguration.FileStructureSource.Returns(fileStructureSource);

                var result = await Builder.IdentifyAsync(data, MediaItemTypes.Series);

                result.IsRight.Should().BeTrue();
                fileStructureSource.Received(1).GetEmbySourceDataLoader(MediaItemTypes.Series);
                embySourceDataLoader.Received(1).LoadFrom(data);
            }

            [Test]
            public async Task LibraryData_UsesLibrarySourceLoader()
            {
                var data = Data.LibraryEmbyItemData();
                var sourceData = Substitute.For<ISourceData>();
                var source = TestSources.AniDbSource;
                sourceData.Source.Returns(source);
                var embySourceDataLoader = Substitute.For<IEmbySourceDataLoader>();
                embySourceDataLoader.LoadFrom(data).Returns(Right<ProcessFailedResult, ISourceData>(sourceData));

                var libraryStructureSource = Substitute.For<ISource>();
                libraryStructureSource.GetEmbySourceDataLoader(MediaItemTypes.Series)
                    .Returns(Right<ProcessFailedResult, IEmbySourceDataLoader>(embySourceDataLoader));

                PluginConfiguration.LibraryStructureSource.Returns(libraryStructureSource);

                var result = await Builder.IdentifyAsync(data, MediaItemTypes.Series);

                result.IsRight.Should().BeTrue();
                libraryStructureSource.Received(1).GetEmbySourceDataLoader(MediaItemTypes.Series);
                embySourceDataLoader.Received(1).LoadFrom(data);
            }
        }
    }

    [TestFixture]
    public class BuildMediaItem : MediaItemBuilderTests
    {
        [SetUp]
        public override void Setup()
        {
            _initialSourceData = Data.SourceData("InitialSource");
            _mediaItem = new MediaItem(Substitute.For<IEmbyItemData>(), MediaItemTypes.Series,
                _initialSourceData);

            _sourceDataLoaders = new[]
            {
                Data.SourceDataLoader(_mediaItem, _initialSourceData, "SourceA"),
                Data.SourceDataLoader(_mediaItem, _initialSourceData, "SourceB"),
                Data.SourceDataLoader(_mediaItem, _initialSourceData, "SourceC")
            }.ToList();

            Builder = new MediaItemBuilder(PluginConfiguration, _sourceDataLoaders, new ConsoleLogManager());
        }

        private IList<ISourceDataLoader> _sourceDataLoaders;
        private IMediaItem _mediaItem;
        private ISourceData _initialSourceData;

        [Test]
        public async Task CallsEveryLoaderThatCanLoadFromExistingData()
        {
            _mediaItem = Substitute.For<IMediaItem>();

            _sourceDataLoaders = new[]
            {
                Data.SourceDataLoader(_mediaItem, _initialSourceData, "SourceA"),
                Data.SourceDataLoader(_mediaItem, _initialSourceData, "SourceB"),
                Data.SourceDataLoader(_mediaItem, _initialSourceData, "SourceC")
            }.ToList();

            var existingLoader = Substitute.For<ISourceDataLoader>();
            var existingSourceData = Data.SourceData("ExistingSource");
            var newLoaders = _sourceDataLoaders.ToList();

            _sourceDataLoaders.Add(existingLoader);

            _mediaItem.GetAllSourceData().Returns(Option<ISourceData>.Some(existingSourceData));
            _mediaItem.AddData(Arg.Any<ISourceData>()).Returns(Right<ProcessFailedResult, IMediaItem>(_mediaItem));

            existingLoader.CanLoadFrom(existingSourceData).Returns(false);
            newLoaders.Iter(l => l.CanLoadFrom(existingSourceData).Returns(true));

            Builder = new MediaItemBuilder(PluginConfiguration, _sourceDataLoaders, new ConsoleLogManager());

            await Builder.BuildMediaItemAsync(_mediaItem);

            newLoaders.Iter(s => s.Received(1).LoadFrom(_mediaItem, existingSourceData));
            existingLoader.DidNotReceive().LoadFrom(_mediaItem, existingSourceData);
        }

        [Test]
        public async Task CombinesOutputFromAllLoaders()
        {
            var expected = new List<ISourceData>();

            _sourceDataLoaders.Iter((i, l) =>
            {
                var loaderSourceData = Data.SourceData("LoaderSource" + i.ToString());

                expected.Add(loaderSourceData);

                l.ClearSubstitute();

                l.CanLoadFrom(_initialSourceData).Returns(true);
                l.CanLoadFrom(Arg.Is<object>(o => o != _initialSourceData)).Returns(false);

                l.LoadFrom(Arg.Any<IMediaItem>(), _initialSourceData)
                    .Returns(Right<ProcessFailedResult, ISourceData>(loaderSourceData));
            });

            var builtMediaItem = await Builder.BuildMediaItemAsync(_mediaItem);

            builtMediaItem.IsRight.Should().BeTrue();
            builtMediaItem.IfRight(mi => _sourceDataLoaders.Iter((index, l) =>
                mi.GetAllSourceData().ElementAt(index).Should().BeSameAs(expected[index])));
        }

        [Test]
        public async Task CombinesOutputFromLoadersThatDependOnOtherLoaders()
        {
            var dependentLoader = Substitute.For<ISourceDataLoader>();
            var dependentSourceData = Data.SourceData("DependentSource");

            var dependencySourceData = Data.SourceData("DependencySource");
            _sourceDataLoaders.Last()
                .LoadFrom(Arg.Any<IMediaItem>(), _initialSourceData)
                .Returns(Right<ProcessFailedResult, ISourceData>(dependencySourceData));

            dependentLoader.CanLoadFrom(dependencySourceData).Returns(true);
            dependentLoader.CanLoadFrom(Arg.Is<object>(o => o != dependencySourceData)).Returns(false);

            dependentLoader.LoadFrom(Arg.Any<IMediaItem>(), dependencySourceData)
                .Returns(x => Right<ProcessFailedResult, ISourceData>(dependentSourceData));

            _sourceDataLoaders.Insert(0, dependentLoader);

            var builtMediaItem = await Builder.BuildMediaItemAsync(_mediaItem);

            builtMediaItem.IsRight.Should().BeTrue();
            builtMediaItem.ValueUnsafe().GetAllSourceData().Should().Contain(dependencySourceData);
        }
    }
}