using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using MediaBrowser.Plugins.AniMetadata.SourceDataLoaders;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests.SourceDataLoaders
{
    [TestFixture]
    public class AniDbSeasonFromEmbyDataTests
    {
        [SetUp]
        public void Setup()
        {
            _aniDbSource = Substitute.For<IAniDbSource>();

            _sources = Substitute.For<ISources>();
            _sources.AniDb.Returns(_aniDbSource);

            _embyItemData = Substitute.For<IEmbyItemData>();
            _embyItemData.Language.Returns("en");

            _aniDbSeriesTitles = new ItemTitleData[] { };
            var aniDbSeriesData = new AniDbSeriesData
            {
                Titles = _aniDbSeriesTitles
            };

            _embyItemData.Identifier.Returns(new ItemIdentifier(67, Option<int>.None, "Name"));
            _aniDbSource.GetSeriesData(_embyItemData, Arg.Any<ProcessResultContext>())
                .Returns(aniDbSeriesData);
        }

        private ISources _sources;
        private IEmbyItemData _embyItemData;
        private IAniDbSource _aniDbSource;
        private ItemTitleData[] _aniDbSeriesTitles;

        [Test]
        public void CanLoadFrom_CorrectItemType_IsTrue()
        {
            var loader = new AniDbSeasonFromEmbyData(_sources);

            loader.CanLoadFrom(MediaItemTypes.Season).Should().BeTrue();
        }

        [Test]
        public void CanLoadFrom_Null_IsFalse()
        {
            var loader = new AniDbSeasonFromEmbyData(_sources);

            loader.CanLoadFrom(null).Should().BeFalse();
        }

        [Test]
        public void CanLoadFrom_WrongItemType_IsFalse()
        {
            var loader = new AniDbSeasonFromEmbyData(_sources);

            loader.CanLoadFrom(MediaItemTypes.Series).Should().BeFalse();
        }

        [Test]
        public async Task LoadFrom_NoIndex_SetsIndexToOne()
        {
            var selectedSeriesTitle = "SeriesTitle";
            _aniDbSource.SelectTitle(_aniDbSeriesTitles, "en", Arg.Any<ProcessResultContext>())
                .Returns(selectedSeriesTitle);

            _embyItemData.Identifier.Returns(new ItemIdentifier(Option<int>.None, Option<int>.None, "Name"));

            var loader = new AniDbSeasonFromEmbyData(_sources);

            var result = await loader.LoadFrom(_embyItemData);

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Data.Should().BeNull());
            result.IfRight(r => r.Source.Should().Be(_sources.AniDb));
            result.IfRight(sd =>
                sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(1, Option<int>.None, "SeriesTitle")));
        }

        [Test]
        public async Task LoadFrom_ReturnsIdentifierOnlySourceDataWithSeriesName()
        {
            var selectedSeriesTitle = "SeriesTitle";
            _aniDbSource.SelectTitle(_aniDbSeriesTitles, "en", Arg.Any<ProcessResultContext>())
                .Returns(selectedSeriesTitle);

            var loader = new AniDbSeasonFromEmbyData(_sources);

            var result = await loader.LoadFrom(_embyItemData);

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Data.Should().BeNull());
            result.IfRight(r => r.Source.Should().Be(_sources.AniDb));
            result.IfRight(sd =>
                sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(67, Option<int>.None, "SeriesTitle")));
        }
    }
}