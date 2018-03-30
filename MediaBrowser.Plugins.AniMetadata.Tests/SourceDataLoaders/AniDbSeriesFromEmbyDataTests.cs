using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using MediaBrowser.Plugins.AniMetadata.SourceDataLoaders;
using MediaBrowser.Plugins.AniMetadata.Tests.TestData;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests.SourceDataLoaders
{
    [TestFixture]
    public class AniDbSeriesFromEmbyDataTests
    {
        [SetUp]
        public void Setup()
        {
            _aniDbSource = Substitute.For<IAniDbSource>();
            _aniDbSource.GetSeriesData(_embyItemData, Arg.Any<ProcessResultContext>())
                .Returns(x => _aniDbSeriesData);

            _sources = Substitute.For<ISources>();
            _sources.AniDb.Returns(_aniDbSource);

            _aniDbClient = Substitute.For<IAniDbClient>();

            _embyItemData = Substitute.For<IEmbyItemData>();
            _embyItemData.Identifier.Returns(new ItemIdentifier(67, 1, "Name"));
            _embyItemData.Language.Returns("en");

            _aniDbSeriesData = new AniDbSeriesData().WithStandardData();
        }

        private ISources _sources;
        private IAniDbClient _aniDbClient;
        private IEmbyItemData _embyItemData;
        private AniDbSeriesData _aniDbSeriesData;
        private IAniDbSource _aniDbSource;

        [Test]
        public void CanLoadFrom_CorrectItemType_IsTrue()
        {
            var loader = new AniDbSeriesFromEmbyData(_aniDbClient, _sources);

            loader.CanLoadFrom(MediaItemTypes.Series).Should().BeTrue();
        }

        [Test]
        public void CanLoadFrom_Null_IsFalse()
        {
            var loader = new AniDbSeriesFromEmbyData(_aniDbClient, _sources);

            loader.CanLoadFrom(null).Should().BeFalse();
        }

        [Test]
        public void CanLoadFrom_WrongItemType_IsFalse()
        {
            var loader = new AniDbSeriesFromEmbyData(_aniDbClient, _sources);

            loader.CanLoadFrom(MediaItemTypes.Season).Should().BeFalse();
        }

        [Test]
        public async Task LoadFrom_CreatesSourceData()
        {
            _aniDbClient.FindSeriesAsync("Name").Returns(_aniDbSeriesData);
            _sources.AniDb.SelectTitle(_aniDbSeriesData.Titles, "en", Arg.Any<ProcessResultContext>())
                .Returns("Title");

            var loader = new AniDbSeriesFromEmbyData(_aniDbClient, _sources);

            var result = await loader.LoadFrom(_embyItemData);

            result.IsRight.Should().BeTrue();
            result.IfRight(sd => sd.Data.Should().Be(_aniDbSeriesData));
            result.IfRight(sd => sd.Source.Should().Be(_sources.AniDb));
            result.IfRight(sd =>
                sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(67, Option<int>.None, "Title")));
        }

        [Test]
        public async Task LoadFrom_NoMatchingSeries_Fails()
        {
            var loader = new AniDbSeriesFromEmbyData(_aniDbClient, _sources);

            var result = await loader.LoadFrom(_embyItemData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to find series in AniDb"));
        }

        [Test]
        public async Task LoadFrom_NoTitle_Fails()
        {
            _aniDbClient.FindSeriesAsync("Name").Returns(_aniDbSeriesData);
            _sources.AniDb.SelectTitle(_aniDbSeriesData.Titles, "en", Arg.Any<ProcessResultContext>())
                .Returns(new ProcessFailedResult("", "", MediaItemTypes.Series, "FailedTitle"));

            var loader = new AniDbSeriesFromEmbyData(_aniDbClient, _sources);

            var result = await loader.LoadFrom(_embyItemData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("FailedTitle"));
        }
    }
}