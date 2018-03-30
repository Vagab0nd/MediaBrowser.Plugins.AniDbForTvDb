using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Mapping;
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
    public class TvDbSeriesFromAniDbTests
    {
        [SetUp]
        public void Setup()
        {
            _embyData = Substitute.For<IEmbyItemData>();
            _embyData.Identifier.Returns(new ItemIdentifier(0, 0, "Name"));

            _mediaItem = Substitute.For<IMediaItem>();
            _mediaItem.EmbyData.Returns(_embyData);
            _mediaItem.ItemType.Returns(MediaItemTypes.Episode);

            _mappingList = Substitute.For<IMappingList>();

            _sources = Substitute.For<ISources>();

            _aniDbSource = Substitute.For<IAniDbSource>();
            _sources.AniDb.Returns(_aniDbSource);

            var tvDbSource = Substitute.For<ITvDbSource>();
            _sources.TvDb.Returns(tvDbSource);

            _aniDbSourceData = Substitute.For<ISourceData<AniDbSeriesData>>();

            _embyData.GetParentId(MediaItemTypes.Series, _aniDbSource).Returns(Option<int>.Some(3));

            _noMappingResult = new ProcessFailedResult("", "", null, "");
            _mappingList.GetSeriesMappingFromAniDb(Arg.Any<int>(), Arg.Any<ProcessResultContext>())
                .Returns(Left<ProcessFailedResult, ISeriesMapping>(_noMappingResult));
        }

        private IMappingList _mappingList;
        private ISources _sources;
        private IMediaItem _mediaItem;
        private IAniDbSource _aniDbSource;
        private ISourceData<AniDbSeriesData> _aniDbSourceData;
        private IEmbyItemData _embyData;
        private ProcessFailedResult _noMappingResult;

        private AniDbSeriesData CreateAniDbSeries(int id, string name)
        {
            var seriesData = new AniDbSeriesData
            {
                Id = id,
                Titles = new[]
                {
                    new ItemTitleData
                    {
                        Title = name
                    }
                }
            };

            _aniDbSourceData.Id.Returns(id);
            _aniDbSourceData.Data.Returns(seriesData);

            return seriesData;
        }

        private ISeriesMapping SetUpSeriesMapping(int aniDbSeriesId, int? tvDbSeriesId)
        {
            var seriesMapping = Substitute.For<ISeriesMapping>();
            seriesMapping.Ids.Returns(new SeriesIds(aniDbSeriesId, tvDbSeriesId.ToOption(), Option<int>.None,
                Option<int>.None));

            _mappingList.GetSeriesMappingFromAniDb(aniDbSeriesId, Arg.Any<ProcessResultContext>())
                .Returns(Right<ProcessFailedResult, ISeriesMapping>(seriesMapping));

            return seriesMapping;
        }

        [Test]
        public void CanLoadFrom_Null_IsFalse()
        {
            var loader = new TvDbSeriesFromAniDb(_sources, _mappingList);

            loader.CanLoadFrom(null).Should().BeFalse();
        }

        [Test]
        public void CanLoadFrom_TypeMatch_IsTrue()
        {
            var loader = new TvDbSeriesFromAniDb(_sources, _mappingList);

            loader.CanLoadFrom(Substitute.For<ISourceData<AniDbSeriesData>>()).Should().BeTrue();
        }

        [Test]
        public void CanLoadFrom_TypeMisMatch_IsFalse()
        {
            var loader = new TvDbSeriesFromAniDb(_sources, _mappingList);

            loader.CanLoadFrom(new object()).Should().BeFalse();
        }

        [Test]
        public async Task LoadFrom_FailsToLoadTvDbSeries_Fails()
        {
            CreateAniDbSeries(342, "SeriesName");
            SetUpSeriesMapping(342, 66);

            _sources.TvDb.GetSeriesData(66, Arg.Any<ProcessResultContext>())
                .Returns(new ProcessFailedResult("", "", MediaItemTypes.Series, "Failed"));

            var loader = new TvDbSeriesFromAniDb(_sources, _mappingList);

            var result = await loader.LoadFrom(_mediaItem, _aniDbSourceData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed"));
        }

        [Test]
        public async Task LoadFrom_MapsToTvDbSeries()
        {
            CreateAniDbSeries(342, "SeriesName");
            SetUpSeriesMapping(342, 66);

            var tvDbSeriesData = TvDbTestData.Series(66);

            _sources.TvDb.GetSeriesData(66, Arg.Any<ProcessResultContext>())
                .Returns(tvDbSeriesData);

            var loader = new TvDbSeriesFromAniDb(_sources, _mappingList);

            var result = await loader.LoadFrom(_mediaItem, _aniDbSourceData);

            result.IsRight.Should().BeTrue();
            result.IfRight(sd => sd.Data.Should().Be(tvDbSeriesData));
            result.IfRight(sd => sd.Source.Should().Be(_sources.TvDb));
            result.IfRight(sd => sd.Id.ValueUnsafe().Should().Be(66));
            result.IfRight(sd => sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(Option<int>.None, Option<int>.None, "")));
        }

        [Test]
        public async Task LoadFrom_NoAniDbIdOnSourceData_Fails()
        {
            _aniDbSourceData.Id.Returns(Option<int>.None);

            var loader = new TvDbSeriesFromAniDb(_sources, _mappingList);

            var result = await loader.LoadFrom(_mediaItem, _aniDbSourceData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f =>
                f.Reason.Should().Be("No AniDb Id found on the AniDb data associated with this media item"));
        }

        [Test]
        public async Task LoadFrom_NoSeriesMapping_Fails()
        {
            CreateAniDbSeries(342, "SeriesName");

            var loader = new TvDbSeriesFromAniDb(_sources, _mappingList);

            var result = await loader.LoadFrom(_mediaItem, _aniDbSourceData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Should().Be(_noMappingResult));
        }

        [Test]
        public async Task LoadFrom_NoTvDbIdOnMapping_Fails()
        {
            CreateAniDbSeries(342, "SeriesName");
            SetUpSeriesMapping(342, null);

            var loader = new TvDbSeriesFromAniDb(_sources, _mappingList);

            var result = await loader.LoadFrom(_mediaItem, _aniDbSourceData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("No TvDb Id found on matching mapping"));
        }
    }
}