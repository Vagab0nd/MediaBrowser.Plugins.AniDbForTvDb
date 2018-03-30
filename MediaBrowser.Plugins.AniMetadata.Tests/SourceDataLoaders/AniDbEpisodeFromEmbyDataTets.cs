using System.Threading.Tasks;
using FluentAssertions;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using MediaBrowser.Plugins.AniMetadata.Providers.AniDb;
using MediaBrowser.Plugins.AniMetadata.SourceDataLoaders;
using MediaBrowser.Plugins.AniMetadata.Tests.TestData;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests.SourceDataLoaders
{
    [TestFixture]
    public class AniDbEpisodeFromEmbyDataTets
    {
        [SetUp]
        public void Setup()
        {
            _sources = Substitute.For<ISources>();

            _embyItemData = Substitute.For<IEmbyItemData>();
            _embyItemData.Identifier.Returns(new ItemIdentifier(67, 1, "Name"));
            _embyItemData.Language.Returns("en");

            var aniDbSource = Substitute.For<IAniDbSource>();
            _sources.AniDb.Returns(aniDbSource);

            _mediaItem = Substitute.For<IMediaItem>();
            _mediaItem.EmbyData.Returns(_embyItemData);
            _mediaItem.ItemType.Returns(MediaItemTypes.Episode);

            _aniDbEpisodeMatcher = Substitute.For<IAniDbEpisodeMatcher>();

            _aniDbSeriesData = new AniDbSeriesData().WithStandardData();
            _aniDbEpisodeData = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "67",
                    RawType = 1
                },
                Titles = new[]
                {
                    new EpisodeTitleData
                    {
                        Title = "Title"
                    }
                }
            };
        }

        private ISources _sources;
        private IAniDbEpisodeMatcher _aniDbEpisodeMatcher;
        private IMediaItem _mediaItem;
        private IEmbyItemData _embyItemData;
        private AniDbSeriesData _aniDbSeriesData;
        private AniDbEpisodeData _aniDbEpisodeData;

        [Test]
        public void CanLoadFrom_CorrectItemType_IsTrue()
        {
            var loader = new AniDbEpisodeFromEmbyData(_sources, _aniDbEpisodeMatcher);

            loader.CanLoadFrom(MediaItemTypes.Episode).Should().BeTrue();
        }

        [Test]
        public void CanLoadFrom_Null_IsFalse()
        {
            var loader = new AniDbEpisodeFromEmbyData(_sources, _aniDbEpisodeMatcher);

            loader.CanLoadFrom(null).Should().BeFalse();
        }

        [Test]
        public void CanLoadFrom_WrongItemType_IsFalse()
        {
            var loader = new AniDbEpisodeFromEmbyData(_sources, _aniDbEpisodeMatcher);

            loader.CanLoadFrom(MediaItemTypes.Season).Should().BeFalse();
        }

        [Test]
        public async Task LoadFrom_CreatesSourceData()
        {
            _sources.AniDb.GetSeriesData(_mediaItem.EmbyData, Arg.Any<ProcessResultContext>())
                .Returns(_aniDbSeriesData);

            _aniDbEpisodeMatcher.FindEpisode(_aniDbSeriesData.Episodes, 1, 67, "Name")
                .Returns(_aniDbEpisodeData);

            _sources.AniDb.SelectTitle(_aniDbEpisodeData.Titles, "en", Arg.Any<ProcessResultContext>())
                .Returns("Title");

            var loader = new AniDbEpisodeFromEmbyData(_sources, _aniDbEpisodeMatcher);

            var result = await loader.LoadFrom(_embyItemData);

            result.IsRight.Should().BeTrue();
            result.IfRight(sd => sd.Data.Should().Be(_aniDbEpisodeData));
            result.IfRight(sd => sd.Source.Should().Be(_sources.AniDb));
            result.IfRight(sd => sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(67, 1, "Title")));
        }

        [Test]
        public async Task LoadFrom_NoFoundEpisode_Fails()
        {
            _sources.AniDb.GetSeriesData(_mediaItem.EmbyData, Arg.Any<ProcessResultContext>())
                .Returns(_aniDbSeriesData);

            var loader = new AniDbEpisodeFromEmbyData(_sources, _aniDbEpisodeMatcher);

            var result = await loader.LoadFrom(_embyItemData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to find episode in AniDb"));
        }

        [Test]
        public async Task LoadFrom_NoSeriesData_Fails()
        {
            _sources.AniDb.GetSeriesData(_mediaItem.EmbyData, Arg.Any<ProcessResultContext>())
                .Returns(new ProcessFailedResult("", "", MediaItemTypes.Episode, "FailedSeriesData"));

            var loader = new AniDbEpisodeFromEmbyData(_sources, _aniDbEpisodeMatcher);

            var result = await loader.LoadFrom(_embyItemData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("FailedSeriesData"));
        }

        [Test]
        public async Task LoadFrom_NoTitle_Fails()
        {
            _sources.AniDb.GetSeriesData(_mediaItem.EmbyData, Arg.Any<ProcessResultContext>())
                .Returns(_aniDbSeriesData);

            _aniDbEpisodeMatcher.FindEpisode(_aniDbSeriesData.Episodes, 1, 67, "Name")
                .Returns(_aniDbEpisodeData);

            _sources.AniDb.SelectTitle(_aniDbEpisodeData.Titles, "en", Arg.Any<ProcessResultContext>())
                .Returns(new ProcessFailedResult("", "", MediaItemTypes.Episode, "FailedTitle"));

            var loader = new AniDbEpisodeFromEmbyData(_sources, _aniDbEpisodeMatcher);

            var result = await loader.LoadFrom(_embyItemData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("FailedTitle"));
        }
    }
}