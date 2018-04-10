using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using MediaBrowser.Plugins.AniMetadata.SourceDataLoaders;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests.Process.Sources
{
    [TestFixture]
    public class AniDbSourceTests
    {
        [SetUp]
        public virtual void Setup()
        {
            _aniDbClient = Substitute.For<IAniDbClient>();
            _configuration = Substitute.For<ITitlePreferenceConfiguration>();
            _titleSelector = Substitute.For<IAniDbTitleSelector>();

            _configuration.TitlePreference.Returns(TitleType.Localized);
            _loaders = new List<IEmbySourceDataLoader>();

            _aniDbSource = new AniDbSource(_aniDbClient, _configuration, _titleSelector, _loaders);
        }

        private IAniDbClient _aniDbClient;
        private ITitlePreferenceConfiguration _configuration;
        private IAniDbTitleSelector _titleSelector;
        private AniDbSource _aniDbSource;
        private IList<IEmbySourceDataLoader> _loaders;

        private EmbyItemData EmbyItemData(string name, int? parentAniDbSeriesId)
        {
            var parentIds = new List<EmbyItemId>();

            if (parentAniDbSeriesId.HasValue)
            {
                parentIds.Add(new EmbyItemId(MediaItemTypes.Series, SourceNames.AniDb, parentAniDbSeriesId.Value));
            }

            return new EmbyItemData(MediaItemTypes.Series,
                new ItemIdentifier(Option<int>.None, Option<int>.None, name),
                null, "en", parentIds);
        }

        [Test]
        public void Name_ReturnsAniDbSourceName()
        {
            _aniDbSource.Name.Should().BeSameAs(SourceNames.AniDb);
        }

        [Test]
        [TestCaseSource(typeof(MediaItemTypeTestCases))]
        public void GetEmbySourceDataLoader_MatchingLoader_ReturnsLoader(IMediaItemType mediaItemType)
        {
            var loader = Substitute.For<IEmbySourceDataLoader>();
            loader.SourceName.Returns(SourceNames.AniDb);
            loader.CanLoadFrom(mediaItemType).Returns(true);

            _loaders.Add(loader);

            var result = _aniDbSource.GetEmbySourceDataLoader(mediaItemType);

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Should().BeSameAs(loader));
        }

        [Test]
        [TestCaseSource(typeof(MediaItemTypeTestCases))]
        public void GetEmbySourceDataLoader_NoMatchingLoader_ReturnsFailed(IMediaItemType mediaItemType)
        {
            var sourceMismatch = Substitute.For<IEmbySourceDataLoader>();
            sourceMismatch.SourceName.Returns(SourceNames.TvDb);
            sourceMismatch.CanLoadFrom(mediaItemType).Returns(true);

            var cannotLoad = Substitute.For<IEmbySourceDataLoader>();
            cannotLoad.SourceName.Returns(SourceNames.AniDb);
            cannotLoad.CanLoadFrom(mediaItemType).Returns(false);

            _loaders.Add(sourceMismatch);
            _loaders.Add(cannotLoad);

            var result = _aniDbSource.GetEmbySourceDataLoader(mediaItemType);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("No Emby source data loader for this source and media item type"));
        }

        [Test]
        public async Task GetSeriesData_NoAniDbIdOnParent_ReturnsFailed()
        {
            var embyItemData = EmbyItemData("Name", null);

            var result = await _aniDbSource.GetSeriesData(embyItemData, new ProcessResultContext("", "", null));

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("No AniDb Id found on parent series"));
        }

        [Test]
        public async Task GetSeriesData_NoSeriesLoaded_ReturnsFailed()
        {
            var embyItemData = EmbyItemData("Name", 56);

            _aniDbClient.GetSeriesAsync(56).Returns(Option<AniDbSeriesData>.None);

            var result = await _aniDbSource.GetSeriesData(embyItemData, new ProcessResultContext("", "", null));

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to load parent series with AniDb Id '56'"));
        }

        [Test]
        public async Task GetSeriesData_ReturnsSeries()
        {
            var embyItemData = EmbyItemData("Name", 56);

            var seriesData = new AniDbSeriesData();

            _aniDbClient.GetSeriesAsync(56).Returns(Option<AniDbSeriesData>.Some(seriesData));

            var result = await _aniDbSource.GetSeriesData(embyItemData, new ProcessResultContext("", "", null));

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Should().BeSameAs(seriesData));
        }

        [Test]
        public void SelectTitle_TitleSelected_ReturnsTitle()
        {
            var titles = new ItemTitleData[] { };

            _titleSelector.SelectTitle(titles, TitleType.Localized, "en")
                .Returns(Option<ItemTitleData>.Some(new ItemTitleData
                {
                    Title = "TitleName"
                }));

            var result = _aniDbSource.SelectTitle(titles, "en", new ProcessResultContext("", "", null));

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Should().Be("TitleName"));
        }

        [Test]
        public void SelectTitle_NoTitleSelected_ReturnsFailed()
        {
            var titles = new ItemTitleData[] { };

            _titleSelector.SelectTitle(titles, TitleType.Localized, "en")
                .Returns(Option<ItemTitleData>.None);

            var result = _aniDbSource.SelectTitle(titles, "en", new ProcessResultContext("", "", null));

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to find a title"));
        }
    }
}