using System;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Mapping;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using MediaBrowser.Plugins.AniMetadata.SourceDataLoaders;
using MediaBrowser.Plugins.AniMetadata.Tests.TestData;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;
using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata.Tests.SourceDataLoaders
{
    [TestFixture]
    public class TvDbEpisodeFromAniDbTests
    {
        [SetUp]
        public void Setup()
        {
            _embyData = Substitute.For<IEmbyItemData>();
            _embyData.Identifier.Returns(new ItemIdentifier(0, 0, "Name"));

            _mediaItem = Substitute.For<IMediaItem>();
            _mediaItem.EmbyData.Returns(_embyData);
            _mediaItem.GetDataFromSource(_aniDbSource).Returns(Option<ISourceData>.Some(_aniDbSourceData));
            _mediaItem.ItemType.Returns(MediaItemTypes.Episode);

            _mappingList = Substitute.For<IMappingList>();

            _sources = Substitute.For<ISources>();

            _aniDbSource = Substitute.For<IAniDbSource>();
            _sources.AniDb.Returns(_aniDbSource);

            var tvDbSource = Substitute.For<ITvDbSource>();
            _sources.TvDb.Returns(tvDbSource);

            _aniDbSourceData = Substitute.For<ISourceData<AniDbEpisodeData>>();
            _aniDbSourceData.Id.Returns(Option<int>.Some(3));

            _embyData.GetParentId(MediaItemTypes.Series, _aniDbSource).Returns(Option<int>.Some(3));

            _episodeMapper = Substitute.For<IEpisodeMapper>();

            _mappingList.GetSeriesMappingFromAniDb(Arg.Any<int>(), Arg.Any<ProcessResultContext>())
                .Returns(Left<ProcessFailedResult, ISeriesMapping>(new ProcessFailedResult("", "", null, "")));
        }

        private IMappingList _mappingList;
        private ISources _sources;
        private IMediaItem _mediaItem;
        private IAniDbSource _aniDbSource;
        private ISourceData<AniDbEpisodeData> _aniDbSourceData;
        private IEmbyItemData _embyData;
        private IEpisodeMapper _episodeMapper;

        private ISeriesMapping SetUpSeriesMapping(int aniDbSeriesId, int tvDbSeriesId)
        {
            var seriesMapping = Substitute.For<ISeriesMapping>();
            seriesMapping.Ids.Returns(new SeriesIds(aniDbSeriesId, tvDbSeriesId, Option<int>.None, Option<int>.None));

            _mappingList.GetSeriesMappingFromAniDb(aniDbSeriesId, Arg.Any<ProcessResultContext>())
                .Returns(Right<ProcessFailedResult, ISeriesMapping>(seriesMapping));

            return seriesMapping;
        }

        private AniDbSeriesData SetUpAniDbSeriesData(int id)
        {
            var seriesData = new AniDbSeriesData().WithStandardData();

            seriesData.Id = id;

            _embyData.GetParentId(MediaItemTypes.Series, _aniDbSource).Returns(id);
            _aniDbSource.GetSeriesData(_mediaItem.EmbyData, Arg.Any<ProcessResultContext>()).Returns(seriesData);

            return seriesData;
        }

        private AniDbEpisodeData SetUpAniDbEpisodeData(int episodeIndex, int season = 1)
        {
            if (season < 0 || season > 1)
            {
                throw new ArgumentOutOfRangeException("Invalid season number");
            }

            var episodeData = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = episodeIndex.ToString(),
                    RawType = season == 0 ? 2 : season
                }
            };

            _aniDbSourceData.Data.Returns(episodeData);

            return episodeData;
        }

        private void SetUpEpisodeMapping(AniDbEpisodeData from, TvDbEpisodeData to, ISeriesMapping seriesMapping)
        {
            _episodeMapper
                .MapAniDbEpisodeAsync(from.EpisodeNumber.Number, seriesMapping, Option<EpisodeGroupMapping>.None)
                .Returns(to);
        }

        private TvDbEpisodeData CreateTvDbEpisodeData(int episodeIndex, int season = 1, string name = "TvDbEpisodeName")
        {
            return TvDbTestData.Episode(3, episodeIndex, season, name: name);
        }

        [Test]
        public void CanLoadFrom_Null_IsFalse()
        {
            var loader = new TvDbEpisodeFromAniDb(_sources, _mappingList, _episodeMapper);

            loader.CanLoadFrom(null).Should().BeFalse();
        }

        [Test]
        public void CanLoadFrom_TypeMatch_IsTrue()
        {
            var loader = new TvDbEpisodeFromAniDb(_sources, _mappingList, _episodeMapper);

            loader.CanLoadFrom(Substitute.For<ISourceData<AniDbEpisodeData>>()).Should().BeTrue();
        }

        [Test]
        public void CanLoadFrom_TypeMisMatch_IsFalse()
        {
            var loader = new TvDbEpisodeFromAniDb(_sources, _mappingList, _episodeMapper);

            loader.CanLoadFrom(new object()).Should().BeFalse();
        }

        [Test]
        public async Task LoadFrom_EpisodeMappingFails_Fails()
        {
            SetUpSeriesMapping(324, 142);

            SetUpAniDbSeriesData(324);

            SetUpAniDbEpisodeData(33);

            _episodeMapper.ClearSubstitute();

            var loader = new TvDbEpisodeFromAniDb(_sources, _mappingList, _episodeMapper);

            var result = await loader.LoadFrom(_mediaItem, _aniDbSourceData);

            result.IsLeft.Should().BeTrue();
        }

        [Test]
        public async Task LoadFrom_HasMappedEpisodeData_ReturnsMappedEpisodeData()
        {
            SetUpAniDbSeriesData(324);

            var seriesMapping = SetUpSeriesMapping(324, 142);

            var aniDbEpisodeData = SetUpAniDbEpisodeData(33);
            var tvDbEpisodeData = CreateTvDbEpisodeData(55, 6);

            SetUpEpisodeMapping(aniDbEpisodeData, tvDbEpisodeData, seriesMapping);

            var loader = new TvDbEpisodeFromAniDb(_sources, _mappingList, _episodeMapper);

            var result = await loader.LoadFrom(_mediaItem, _aniDbSourceData);

            result.IsRight.Should().BeTrue();

            result.IfRight(sd => sd.Data.Should().Be(tvDbEpisodeData));
            result.IfRight(sd => sd.Source.Should().Be(_sources.TvDb));
            result.IfRight(sd => sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(55, 6, "TvDbEpisodeName")));
        }

        [Test]
        public async Task LoadFrom_HasSeriesMapping_GetsEpisodeGroupMapping()
        {
            var seriesMapping = SetUpSeriesMapping(324, 142);

            SetUpAniDbSeriesData(324);

            var aniDbEpisodeData = SetUpAniDbEpisodeData(33);

            var loader = new TvDbEpisodeFromAniDb(_sources, _mappingList, _episodeMapper);

            await loader.LoadFrom(_mediaItem, _aniDbSourceData);

            seriesMapping.Received(1).GetEpisodeGroupMapping(aniDbEpisodeData.EpisodeNumber);
        }

        [Test]
        public async Task LoadFrom_NoAniDbSeriesData_Fails()
        {
            SetUpSeriesMapping(324, 142);

            SetUpAniDbSeriesData(324);

            SetUpAniDbEpisodeData(33);

            _aniDbSource.ClearSubstitute();
            _aniDbSource.GetSeriesData(_mediaItem.EmbyData, Arg.Any<ProcessResultContext>())
                .Returns(Left<ProcessFailedResult, AniDbSeriesData>(new ProcessFailedResult("", "",
                    MediaItemTypes.Series,
                    "Failed")));

            var loader = new TvDbEpisodeFromAniDb(_sources, _mappingList, _episodeMapper);

            var result = await loader.LoadFrom(_mediaItem, _aniDbSourceData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(r => r.Reason.Should().Be("Failed"));
        }

        [Test]
        public async Task LoadFrom_NoMappedEpisodeData_Fails()
        {
            SetUpSeriesMapping(324, 142);

            SetUpAniDbSeriesData(324);

            SetUpAniDbEpisodeData(33);

            var loader = new TvDbEpisodeFromAniDb(_sources, _mappingList, _episodeMapper);

            var result = await loader.LoadFrom(_mediaItem, _aniDbSourceData);

            result.IsLeft.Should().BeTrue();
        }

        [Test]
        public async Task LoadFrom_NoSeriesMapping_Fails()
        {
            SetUpAniDbSeriesData(324);

            SetUpAniDbEpisodeData(33);

            var loader = new TvDbEpisodeFromAniDb(_sources, _mappingList, _episodeMapper);

            var result = await loader.LoadFrom(_mediaItem, _aniDbSourceData);

            result.IsLeft.Should().BeTrue();
        }
    }
}