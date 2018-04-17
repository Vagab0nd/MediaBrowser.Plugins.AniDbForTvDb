using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.AniDb.Titles;
using MediaBrowser.Plugins.AniMetadata.AniList;
using MediaBrowser.Plugins.AniMetadata.AniList.Data;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using MediaBrowser.Plugins.AniMetadata.SourceDataLoaders;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
using NSubstitute;
using NUnit.Framework;
using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata.Tests.SourceDataLoaders
{
    [TestFixture]
    public class AniListSeriesFromAniDbTests
    {
        [SetUp]
        public void Setup()
        {
            _aniListClient = Substitute.For<IAniListClient>();
            _aniListClient.FindSeriesAsync(null, null)
                .ReturnsForAnyArgs(
                    Left<ProcessFailedResult, IEnumerable<AniListSeriesData>>(
                        TestProcessResultContext.Failed("Failed")));

            _sources = new TestSources();

            _titleNormaliser = Substitute.For<ITitleNormaliser>();
            _titleNormaliser.GetNormalisedTitle(Arg.Any<string>()).Returns(x => $"Normalised{x.Arg<string>()}");

            _embyData = Substitute.For<IEmbyItemData>();
            _embyData.Identifier.Returns(new ItemIdentifier(0, 0, "Name"));
            _embyData.Language.Returns("en");

            _mediaItem = Substitute.For<IMediaItem>();
            _mediaItem.EmbyData.Returns(_embyData);
            _mediaItem.ItemType.Returns(MediaItemTypes.Series);

            _aniDbTitles = new ItemTitleData[] { };
            _aniDbSourceData = Substitute.For<ISourceData<AniDbSeriesData>>();
            _aniDbSourceData.Data.Returns(x => new AniDbSeriesData
            {
                Titles = _aniDbTitles
            });


            _loader = new AniListSeriesFromAniDb(_aniListClient, _sources, _titleNormaliser);
        }

        private IAniListClient _aniListClient;
        private ISources _sources;
        private ITitleNormaliser _titleNormaliser;
        private AniListSeriesFromAniDb _loader;
        private IMediaItem _mediaItem;
        private IEmbyItemData _embyData;
        private ISourceData<AniDbSeriesData> _aniDbSourceData;
        private ItemTitleData[] _aniDbTitles;

        [Test]
        public void CanLoadFrom_Null_IsFalse()
        {
            _loader.CanLoadFrom(null).Should().BeFalse();
        }

        [Test]
        public void CanLoadFrom_TypeMatch_IsTrue()
        {
            _loader.CanLoadFrom(Substitute.For<ISourceData<AniDbSeriesData>>()).Should().BeTrue();
        }

        [Test]
        public void CanLoadFrom_TypeMisMatch_IsFalse()
        {
            _loader.CanLoadFrom(new object()).Should().BeFalse();
        }

        [Test]
        public async Task LoadFrom_NoAniListSeriesFoundForFirstAniDbTitles_QueriesForSecondTitle()
        {
            _aniDbTitles = new[]
            {
                new ItemTitleData
                {
                    Title = "TitleA",
                    Language = "en"
                },
                new ItemTitleData
                {
                    Title = "TitleB",
                    Language = "en"
                }
            };

            _aniListClient.FindSeriesAsync("NormalisedTitleA", Arg.Any<ProcessResultContext>())
                .Returns(new AniListSeriesData[] { });

            _aniListClient.FindSeriesAsync("NormalisedTitleB", Arg.Any<ProcessResultContext>())
                .Returns(new AniListSeriesData[] { });

            await _loader.LoadFrom(_mediaItem, _aniDbSourceData);

            _aniListClient.Received(1).FindSeriesAsync("NormalisedTitleA", Arg.Any<ProcessResultContext>());
            _aniListClient.Received(1).FindSeriesAsync("NormalisedTitleB", Arg.Any<ProcessResultContext>());
        }

        [Test]
        public async Task LoadFrom_MultipleDistinctSeriesForOneTitle_Fails()
        {
            _aniDbTitles = new[]
            {
                new ItemTitleData
                {
                    Title = "TitleA",
                    Language = "en"
                }
            };

            _aniListClient.FindSeriesAsync("NormalisedTitleA", Arg.Any<ProcessResultContext>())
                .Returns(new[]
                {
                    new AniListSeriesData
                    {
                        Id = 1
                    },
                    new AniListSeriesData
                    {
                        Id = 2
                    }
                });

            var result = await _loader.LoadFrom(_mediaItem, _aniDbSourceData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(r => r.Reason.Should().Be("Found too many (2) matching AniList series"));
        }

        [Test]
        public async Task LoadFrom_SingleDistinctSeriesFound_ReturnsSourceData()
        {
            var aniListSeriesData = new AniListSeriesData
            {
                Id = 1,
                Title = new AniListTitleData("english", "", "")
            };

            _aniDbTitles = new[]
            {
                new ItemTitleData
                {
                    Title = "TitleA",
                    Language = "en"
                }
            };

            _aniListClient.FindSeriesAsync("NormalisedTitleA", Arg.Any<ProcessResultContext>())
                .Returns(new[]
                {
                    aniListSeriesData
                });

            _sources.AniList.SelectTitle(aniListSeriesData.Title, "en", Arg.Any<ProcessResultContext>())
                .Returns(Right<ProcessFailedResult, string>("SelectedTitle"));

            var result = await _loader.LoadFrom(_mediaItem, _aniDbSourceData);

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Should()
                .BeEquivalentTo(new SourceData<AniListSeriesData>(
                    TestSources.AniListSource,
                    1,
                    new ItemIdentifier(None, None, "SelectedTitle"),
                    aniListSeriesData
                )));
        }

        [Test]
        public void SourceName_ReturnsAniList()
        {
            _loader.SourceName.Should().Be(SourceNames.AniList);
        }
    }
}