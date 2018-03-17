using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
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
    public class EmbySeriesToTvDbTests
    {
        [SetUp]
        public void Setup()
        {
            var tvDbSource = Substitute.For<ITvDbSource>();

            _sources = Substitute.For<ISources>();
            _sources.TvDb.Returns(tvDbSource);

            _tvDbClient = Substitute.For<ITvDbClient>();

            _embyItemData = Substitute.For<IEmbyItemData>();
            _embyItemData.Identifier.Returns(new ItemIdentifier(2, 1, "SeriesName"));
        }

        private ITvDbClient _tvDbClient;
        private ISources _sources;
        private IEmbyItemData _embyItemData;

        [Test]
        public void CanLoadFrom_CorrectItemType_IsTrue()
        {
            var loader = new EmbySeriesToTvDb(_tvDbClient, _sources);

            loader.CanLoadFrom(MediaItemTypes.Series).Should().BeTrue();
        }

        [Test]
        public void CanLoadFrom_Null_IsFalse()
        {
            var loader = new EmbySeriesToTvDb(_tvDbClient, _sources);

            loader.CanLoadFrom(null).Should().BeFalse();
        }

        [Test]
        public void CanLoadFrom_WrongItemType_IsFalse()
        {
            var loader = new EmbySeriesToTvDb(_tvDbClient, _sources);

            loader.CanLoadFrom(MediaItemTypes.Season).Should().BeFalse();
        }

        [Test]
        public async Task LoadFrom_CreatesSourceData()
        {
            var tvDbSeriesData = TvDbTestData.Series(22, "SeriesName");
            _tvDbClient.FindSeriesAsync("SeriesName").Returns(tvDbSeriesData);

            var loader = new EmbySeriesToTvDb(_tvDbClient, _sources);

            var result = await loader.LoadFrom(_embyItemData);

            result.IsRight.Should().BeTrue();

            result.IfRight(sd => sd.Data.Should().Be(tvDbSeriesData));
            result.IfRight(sd => sd.Source.Should().Be(_sources.TvDb));
            result.IfRight(sd =>
                sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(2, Option<int>.None, "SeriesName")));
        }

        [Test]
        public async Task LoadFrom_NoFoundSeries_Fails()
        {
            var loader = new EmbySeriesToTvDb(_tvDbClient, _sources);

            var result = await loader.LoadFrom(_embyItemData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to find series in TvDb"));
        }
    }
}