using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using MediaBrowser.Plugins.AniMetadata.SourceDataLoaders;
using MediaBrowser.Plugins.AniMetadata.Tests.TestData;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests.Process.Sources
{
    [TestFixture]
    public class TvDbSourceTests
    {
        [SetUp]
        public virtual void Setup()
        {
            _tvDbClient = Substitute.For<ITvDbClient>();
            _loaders = new List<IEmbySourceDataLoader>();

            _tvDbSource = new TvDbSource(_tvDbClient, _loaders);
        }

        private ITvDbClient _tvDbClient;
        private TvDbSource _tvDbSource;
        private IList<IEmbySourceDataLoader> _loaders;

        private EmbyItemData EmbyItemData(string name, int? parentTvDbSeriesId)
        {
            var parentIds = new List<EmbyItemId>();

            if (parentTvDbSeriesId.HasValue)
            {
                parentIds.Add(new EmbyItemId(MediaItemTypes.Series, SourceNames.TvDb, parentTvDbSeriesId.Value));
            }

            return new EmbyItemData(MediaItemTypes.Episode,
                new ItemIdentifier(Option<int>.None, Option<int>.None, name),
                null, "en", parentIds);
        }

        private EmbyItemData SeriesEmbyItemData(string name, int? tvDbSeriesId)
        {
            var existingIds = new Dictionary<string, int>();

            if (tvDbSeriesId.HasValue)
            {
                existingIds.Add(SourceNames.TvDb, tvDbSeriesId.Value);
            }

            return new EmbyItemData(MediaItemTypes.Series,
                new ItemIdentifier(Option<int>.None, Option<int>.None, name),
                existingIds, "en", new List<EmbyItemId>());
        }

        [Test]
        public void Name_ReturnsTvDbSourceName()
        {
            _tvDbSource.Name.Should().BeSameAs(SourceNames.TvDb);
        }

        [Test]
        [TestCaseSource(typeof(MediaItemTypeTestCases))]
        public void GetEmbySourceDataLoader_MatchingLoader_ReturnsLoader(IMediaItemType mediaItemType)
        {
            var loader = Substitute.For<IEmbySourceDataLoader>();
            loader.SourceName.Returns(SourceNames.TvDb);
            loader.CanLoadFrom(mediaItemType).Returns(true);

            _loaders.Add(loader);

            var result = _tvDbSource.GetEmbySourceDataLoader(mediaItemType);

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Should().BeSameAs(loader));
        }

        [Test]
        [TestCaseSource(typeof(MediaItemTypeTestCases))]
        public void GetEmbySourceDataLoader_NoMatchingLoader_ReturnsFailed(IMediaItemType mediaItemType)
        {
            var sourceMismatch = Substitute.For<IEmbySourceDataLoader>();
            sourceMismatch.SourceName.Returns(SourceNames.AniDb);
            sourceMismatch.CanLoadFrom(mediaItemType).Returns(true);

            var cannotLoad = Substitute.For<IEmbySourceDataLoader>();
            cannotLoad.SourceName.Returns(SourceNames.TvDb);
            cannotLoad.CanLoadFrom(mediaItemType).Returns(false);

            _loaders.Add(sourceMismatch);
            _loaders.Add(cannotLoad);

            var result = _tvDbSource.GetEmbySourceDataLoader(mediaItemType);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("No Emby source data loader for this source and media item type"));
        }

        [Test]
        public async Task GetSeriesData_SeriesMediaItem_NoExistingId_ReturnsFailed()
        {
            var embyItemData = SeriesEmbyItemData("Name", null);

            var result = await _tvDbSource.GetSeriesData(embyItemData, TestProcessResultContext.Instance);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("No TvDb Id found on this series"));
        }

        [Test]
        public async Task GetSeriesData_SeriesMediaItem_NoSeriesLoaded_ReturnsFailed()
        {
            var embyItemData = SeriesEmbyItemData("Name", 56);

            _tvDbClient.GetSeriesAsync(56).Returns(Option<TvDbSeriesData>.None);

            var result = await _tvDbSource.GetSeriesData(embyItemData, TestProcessResultContext.Instance);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to load parent series with TvDb Id '56'"));
        }

        [Test]
        public async Task GetSeriesData_NoTvDbIdOnParent_ReturnsFailed()
        {
            var embyItemData = EmbyItemData("Name", null);

            var result = await _tvDbSource.GetSeriesData(embyItemData, TestProcessResultContext.Instance);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("No TvDb Id found on parent series"));
        }

        [Test]
        public async Task GetSeriesData_NoSeriesLoaded_ReturnsFailed()
        {
            var embyItemData = EmbyItemData("Name", 56);

            _tvDbClient.GetSeriesAsync(56).Returns(Option<TvDbSeriesData>.None);

            var result = await _tvDbSource.GetSeriesData(embyItemData, TestProcessResultContext.Instance);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to load parent series with TvDb Id '56'"));
        }

        [Test]
        public async Task GetSeriesData_SeriesMediaItem_ReturnsSeries()
        {
            var embyItemData = SeriesEmbyItemData("Name", 56);

            var seriesData = TvDbTestData.Series(56, "Name");

            _tvDbClient.GetSeriesAsync(56).Returns(Option<TvDbSeriesData>.Some(seriesData));

            var result = await _tvDbSource.GetSeriesData(embyItemData, TestProcessResultContext.Instance);

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Should().BeSameAs(seriesData));
        }

        [Test]
        public async Task GetSeriesData_ReturnsSeries()
        {
            var embyItemData = EmbyItemData("Name", 56);

            var seriesData = TvDbTestData.Series(56, "Name");

            _tvDbClient.GetSeriesAsync(56).Returns(Option<TvDbSeriesData>.Some(seriesData));

            var result = await _tvDbSource.GetSeriesData(embyItemData, TestProcessResultContext.Instance);

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Should().BeSameAs(seriesData));
        }
    }
}