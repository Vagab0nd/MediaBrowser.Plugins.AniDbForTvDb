using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using MediaBrowser.Plugins.AniMetadata.SourceDataLoaders;
using MediaBrowser.Plugins.AniMetadata.Tests.TestData;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;
using NSubstitute;
using NUnit.Framework;
using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata.Tests.SourceDataLoaders
{
    [TestFixture]
    public class TvDbSeriesFromTvDbEpisodeTests
    {
        [SetUp]
        public void Setup()
        {
            _tvDbSource = Substitute.For<ITvDbSource>();
            _tvDbSource.Name.Returns(SourceNames.TvDb);

            _sources = Substitute.For<ISources>();
            _sources.TvDb.Returns(_tvDbSource);

            var embyItemData = Substitute.For<IEmbyItemData>();
            embyItemData.Identifier.Returns(new ItemIdentifier(67, 1, "Name"));
            embyItemData.Language.Returns("en");

            _mediaItem = Substitute.For<IMediaItem>();
            _mediaItem.EmbyData.Returns(embyItemData);

            _tvDbSeriesData = TvDbTestData.Series(3, "Title");

            _tvDbSource.GetSeriesData(_mediaItem.EmbyData, Arg.Any<ProcessResultContext>())
                .Returns(_tvDbSeriesData);
        }

        private ISources _sources;
        private TvDbSeriesData _tvDbSeriesData;
        private ITvDbSource _tvDbSource;
        private IMediaItem _mediaItem;

        [Test]
        public void CanLoadFrom_CorrectItemType_IsTrue()
        {
            var sourceData = Substitute.For<ISourceData<TvDbEpisodeData>>();

            var loader = new TvDbSeriesFromTvDbEpisode(_sources);

            loader.CanLoadFrom(sourceData).Should().BeTrue();
        }

        [Test]
        public void CanLoadFrom_Null_IsFalse()
        {
            var loader = new TvDbSeriesFromTvDbEpisode(_sources);

            loader.CanLoadFrom(null).Should().BeFalse();
        }

        [Test]
        public void CanLoadFrom_WrongItemType_IsFalse()
        {
            var sourceData = Substitute.For<ISourceData<TvDbSeriesData>>();

            var loader = new TvDbSeriesFromTvDbEpisode(_sources);

            loader.CanLoadFrom(sourceData).Should().BeFalse();
        }

        [Test]
        public async Task LoadFrom_CreatesSourceData()
        {
            var loader = new TvDbSeriesFromTvDbEpisode(_sources);

            var result = await loader.LoadFrom(_mediaItem, null);

            result.IsRight.Should().BeTrue();
            result.IfRight(sd => sd.Data.Should().Be(_tvDbSeriesData));
            result.IfRight(sd => sd.Source.Should().BeEquivalentTo(_sources.TvDb.ForAdditionalData()));
            result.IfRight(sd =>
                sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(67, Option<int>.None, "Title")));
        }

        [Test]
        public async Task LoadFrom_NoMatchingSeries_Fails()
        {
            _tvDbSource.GetSeriesData(_mediaItem.EmbyData, Arg.Any<ProcessResultContext>())
                .Returns(Left<ProcessFailedResult, TvDbSeriesData>(new ProcessFailedResult("", "",
                    MediaItemTypes.Series, "Failed to find series in TvDb")));

            var loader = new TvDbSeriesFromTvDbEpisode(_sources);

            var result = await loader.LoadFrom(_mediaItem, null);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to find series in TvDb"));
        }
    }
}