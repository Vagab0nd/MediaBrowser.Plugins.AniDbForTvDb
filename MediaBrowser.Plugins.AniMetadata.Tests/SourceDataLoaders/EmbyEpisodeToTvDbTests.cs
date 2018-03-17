using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb.Titles;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using MediaBrowser.Plugins.AniMetadata.SourceDataLoaders;
using MediaBrowser.Plugins.AniMetadata.Tests.TestData;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests.SourceDataLoaders
{
    [TestFixture]
    public class EmbyEpisodeToTvDbTests
    {
        [SetUp]
        public void Setup()
        {
            _sources = Substitute.For<ISources>();

            var tvDbSource = Substitute.For<ITvDbSource>();
            _sources.TvDb.Returns(tvDbSource);

            _embyItemData = Substitute.For<IEmbyItemData>();
            _embyItemData.Language.Returns("en");
            _embyItemData.GetParentId(MediaItemTypes.Series, _sources.TvDb).Returns(22);

            _mediaItem = Substitute.For<IMediaItem>();
            _mediaItem.EmbyData.Returns(_embyItemData);
            _mediaItem.ItemType.Returns(MediaItemTypes.Episode);

            _tvDbClient = Substitute.For<ITvDbClient>();
            _titleNormaliser = new TitleNormaliser();
        }

        private ISources _sources;
        private IMediaItem _mediaItem;
        private IEmbyItemData _embyItemData;
        private ITvDbClient _tvDbClient;
        private ITitleNormaliser _titleNormaliser;

        [Test]
        public void CanLoadFrom_CorrectItemType_IsTrue()
        {
            var loader = new EmbyEpisodeToTvDb(_sources, _tvDbClient, _titleNormaliser);

            loader.CanLoadFrom(MediaItemTypes.Episode).Should().BeTrue();
        }

        [Test]
        public void CanLoadFrom_Null_IsFalse()
        {
            var loader = new EmbyEpisodeToTvDb(_sources, _tvDbClient, _titleNormaliser);

            loader.CanLoadFrom(null).Should().BeFalse();
        }

        [Test]
        public void CanLoadFrom_WrongItemType_IsFalse()
        {
            var loader = new EmbyEpisodeToTvDb(_sources, _tvDbClient, _titleNormaliser);

            loader.CanLoadFrom(MediaItemTypes.Season).Should().BeFalse();
        }
        
        [Test]
        public async Task LoadFrom_NoSeriesId_Fails()
        {
            _embyItemData.GetParentId(MediaItemTypes.Series, _sources.TvDb).Returns(Option<int>.None);

            var loader = new EmbyEpisodeToTvDb(_sources, _tvDbClient, _titleNormaliser);

            var result = await loader.LoadFrom(_embyItemData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("No TvDb Id found on parent series"));
        }

        [Test]
        public async Task LoadFrom_EpisodeLoadFail_Fails()
        {
            _embyItemData.Identifier.Returns(new ItemIdentifier(4, 1, "Name"));
            
            var loader = new EmbyEpisodeToTvDb(_sources, _tvDbClient, _titleNormaliser);

            var result = await loader.LoadFrom(_embyItemData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to load parent series with TvDb Id '22'"));
        }

        [Test]
        public async Task LoadFrom_NoMatchingEpisode_Fails()
        {
            _embyItemData.Identifier.Returns(new ItemIdentifier(4, 1, "Name"));

            _tvDbClient.GetEpisodesAsync(22)
                .Returns(new[]
                {
                    TvDbTestData.Episode(1, 1, 1, name: "NonMatch1"),
                    TvDbTestData.Episode(1, 4, 2, name: "NonMatch2")
                }.ToList());

            var loader = new EmbyEpisodeToTvDb(_sources, _tvDbClient, _titleNormaliser);

            var result = await loader.LoadFrom(_embyItemData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to find TvDb episode"));
        }

        [Test]
        public async Task LoadFrom_MatchOnEpisodeAndSeasonIndex_CreatesSourceData()
        {
            var expected = TvDbTestData.Episode(1, 4, 2, name: "Match");

            _embyItemData.Identifier.Returns(new ItemIdentifier(4, 2, "Name"));

            _tvDbClient.GetEpisodesAsync(22)
                .Returns(new[]
                {
                    TvDbTestData.Episode(1, 1, 1, name: "NonMatch"),
                    expected
                }.ToList());

            var loader = new EmbyEpisodeToTvDb(_sources, _tvDbClient, _titleNormaliser);

            var result = await loader.LoadFrom(_embyItemData);

            result.IsRight.Should().BeTrue();
            result.IfRight(sd => sd.Data.Should().Be(expected));
            result.IfRight(sd => sd.Source.Should().Be(_sources.TvDb));
            result.IfRight(sd => sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(4, 2, "Match")));
        }

        [Test]
        public async Task LoadFrom_MatchOnEpisodeAndDefaultSeasonIndex_CreatesSourceData()
        {
            var expected = TvDbTestData.Episode(1, 4, 1, name: "Match");

            _embyItemData.Identifier.Returns(new ItemIdentifier(4, Option<int>.None, "Name"));

            _tvDbClient.GetEpisodesAsync(22)
                .Returns(new[]
                {
                    TvDbTestData.Episode(1, 1, 2, name: "NonMatch"),
                    expected
                }.ToList());

            var loader = new EmbyEpisodeToTvDb(_sources, _tvDbClient, _titleNormaliser);

            var result = await loader.LoadFrom(_embyItemData);

            result.IsRight.Should().BeTrue();
            result.IfRight(sd => sd.Data.Should().Be(expected));
            result.IfRight(sd => sd.Source.Should().Be(_sources.TvDb));
            result.IfRight(sd => sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(4, 1, "Match")));
        }

        [Test]
        public async Task LoadFrom_MatchOnTitle_CreatesSourceData()
        {
            var expected = TvDbTestData.Episode(1, 6, 1, name: "Match");

            _embyItemData.Identifier.Returns(new ItemIdentifier(4, 2, "Match"));

            _tvDbClient.GetEpisodesAsync(22)
                .Returns(new[]
                {
                    TvDbTestData.Episode(1, 1, 1, name: "NonMatch"),
                    expected
                }.ToList());

            var loader = new EmbyEpisodeToTvDb(_sources, _tvDbClient, _titleNormaliser);

            var result = await loader.LoadFrom(_embyItemData);

            result.IsRight.Should().BeTrue();
            result.IfRight(sd => sd.Data.Should().Be(expected));
            result.IfRight(sd => sd.Source.Should().Be(_sources.TvDb));
            result.IfRight(sd => sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(6, 1, "Match")));
        }

    }
}