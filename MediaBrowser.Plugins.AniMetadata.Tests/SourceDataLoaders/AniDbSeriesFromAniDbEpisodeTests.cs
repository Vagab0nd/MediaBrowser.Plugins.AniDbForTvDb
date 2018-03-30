using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using MediaBrowser.Plugins.AniMetadata.SourceDataLoaders;
using MediaBrowser.Plugins.AniMetadata.Tests.TestData;
using NSubstitute;
using NUnit.Framework;
using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata.Tests.SourceDataLoaders
{
    [TestFixture]
    public class AniDbSeriesFromAniDbEpisodeTests
    {
        [SetUp]
        public void Setup()
        {
            _aniDbSource = Substitute.For<IAniDbSource>();
            _aniDbSource.Name.Returns(SourceNames.AniDb);

            _sources = Substitute.For<ISources>();
            _sources.AniDb.Returns(_aniDbSource);

            var embyItemData = Substitute.For<IEmbyItemData>();
            embyItemData.Identifier.Returns(new ItemIdentifier(67, 1, "Name"));
            embyItemData.Language.Returns("en");

            _mediaItem = Substitute.For<IMediaItem>();
            _mediaItem.EmbyData.Returns(embyItemData);

            _aniDbSeriesData = new AniDbSeriesData().WithStandardData();

            _aniDbSource.GetSeriesData(embyItemData, Arg.Any<ProcessResultContext>())
                .Returns(_aniDbSeriesData);
        }

        private ISources _sources;
        private AniDbSeriesData _aniDbSeriesData;
        private IAniDbSource _aniDbSource;
        private IMediaItem _mediaItem;

        [Test]
        public void CanLoadFrom_CorrectItemType_IsTrue()
        {
            var sourceData = Substitute.For<ISourceData<AniDbEpisodeData>>();

            var loader = new AniDbSeriesFromAniDbEpisode(_sources);

            loader.CanLoadFrom(sourceData).Should().BeTrue();
        }

        [Test]
        public void CanLoadFrom_Null_IsFalse()
        {
            var loader = new AniDbSeriesFromAniDbEpisode(_sources);

            loader.CanLoadFrom(null).Should().BeFalse();
        }

        [Test]
        public void CanLoadFrom_WrongItemType_IsFalse()
        {
            var sourceData = Substitute.For<ISourceData<AniDbSeriesData>>();

            var loader = new AniDbSeriesFromAniDbEpisode(_sources);

            loader.CanLoadFrom(sourceData).Should().BeFalse();
        }

        [Test]
        public async Task LoadFrom_CreatesSourceData()
        {
            _sources.AniDb.SelectTitle(_aniDbSeriesData.Titles, "en", Arg.Any<ProcessResultContext>())
                .Returns("Title");

            var loader = new AniDbSeriesFromAniDbEpisode(_sources);

            var result = await loader.LoadFrom(_mediaItem, null);

            result.IsRight.Should().BeTrue();
            result.IfRight(sd => sd.Data.Should().Be(_aniDbSeriesData));
            result.IfRight(sd => sd.Source.Should().BeEquivalentTo(_sources.AniDb.ForAdditionalData()));
            result.IfRight(sd =>
                sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(67, Option<int>.None, "Title")));
        }

        [Test]
        public async Task LoadFrom_NoMatchingSeries_Fails()
        {
            _aniDbSource.GetSeriesData(_mediaItem.EmbyData, Arg.Any<ProcessResultContext>())
                .Returns(Left<ProcessFailedResult, AniDbSeriesData>(new ProcessFailedResult("", "",
                    MediaItemTypes.Series, "Failed to find series in AniDb")));

            var loader = new AniDbSeriesFromAniDbEpisode(_sources);

            var result = await loader.LoadFrom(_mediaItem, null);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to find series in AniDb"));
        }
    }
}