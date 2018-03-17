using System;
using System.Collections.Generic;
using System.Text;
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
using NUnit.Framework;
using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata.Tests.SourceDataLoaders
{
    [TestFixture]
    public class TvDbEpisodeToAniDbTests
    {
        private IEpisodeMapper _episodeMapper;
        private ISources _sources;
        private IMappingList _mappingList;
        private IEmbyItemData _embyData;
        private IMediaItem _mediaItem;
        private ISourceData<TvDbEpisodeData> _tvDbSourceData;
        private ProcessFailedResult _noMappingResult;

        [SetUp]
        public void Setup()
        {
            _episodeMapper = Substitute.For<IEpisodeMapper>();
            _sources = Substitute.For<ISources>();
            _mappingList = Substitute.For<IMappingList>();

            _embyData = Substitute.For<IEmbyItemData>();
            _embyData.Identifier.Returns(new ItemIdentifier(67, 53, "Name"));
            _embyData.Language.Returns("en");

            var aniDbSource = Substitute.For<IAniDbSource>();
            _sources.AniDb.Returns(aniDbSource);

            var tvDbSource = Substitute.For<ITvDbSource>();
            _sources.TvDb.Returns(tvDbSource);

            _tvDbSourceData = Substitute.For<ISourceData<TvDbEpisodeData>>();

            _mediaItem = Substitute.For<IMediaItem>();
            _mediaItem.EmbyData.Returns(_embyData);
            _mediaItem.ItemType.Returns(MediaItemTypes.Episode);

            SetUpTvDbEpisodeData(56, 67, 53);
            
            _noMappingResult = new ProcessFailedResult("", "", null, "");
            _mappingList.GetSeriesMappingsFromTvDb(Arg.Any<int>(), Arg.Any<ProcessResultContext>())
                .Returns(Left<ProcessFailedResult, IEnumerable<ISeriesMapping>>(_noMappingResult));
        }

        private TvDbSeriesData SetUpTvDbSeriesData(int id)
        {
            var seriesData = TvDbTestData.Series(id);
            
            _embyData.GetParentId(MediaItemTypes.Series, _sources.TvDb).Returns(id);
            _sources.TvDb.GetSeriesData(_embyData, Arg.Any<ProcessResultContext>()).Returns(seriesData);

            return seriesData;
        }

        private TvDbEpisodeData SetUpTvDbEpisodeData(int id, int tvDbEpisodeIndex, int tvDbSeasonIndex)
        {
            var episodeData = TvDbTestData.Episode(id, tvDbEpisodeIndex, tvDbSeasonIndex);

            _embyData.GetParentId(MediaItemTypes.Series, _sources.TvDb).Returns(id);
            _tvDbSourceData.Data.Returns(episodeData);

            return episodeData;
        }

        private ISeriesMapping CreateSeriesMapping(int tvDbSeriesId, int aniDbSeriesId)
        {
            var mapping = Substitute.For<ISeriesMapping>();

            mapping.Ids.Returns(new SeriesIds(aniDbSeriesId, tvDbSeriesId, Option<int>.None, Option<int>.None));

            return mapping;
        }

        private AniDbEpisodeData SetUpEpisodeMapping(int tvDbEpisodeIndex, int aniDbEpisodeIndex, ISeriesMapping seriesMapping, string title)
        {
            var aniDbEpisodeData = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = aniDbEpisodeIndex.ToString(),
                    RawType = 1
                },
                Titles = new []
                {
                    new EpisodeTitleData
                    {
                        Title = title
                    }
                }
            };

            _sources.AniDb.SelectTitle(aniDbEpisodeData.Titles, "en", Arg.Any<ProcessResultContext>())
                .Returns(title);

            _episodeMapper.MapTvDbEpisodeAsync(tvDbEpisodeIndex, seriesMapping, Option<EpisodeGroupMapping>.None)
                .Returns(aniDbEpisodeData);

            return aniDbEpisodeData;
        }

        [Test]
        public void CanLoadFrom_Null_IsFalse()
        {
            var loader = new TvDbEpisodeToAniDb(_sources, _mappingList, _episodeMapper);

            loader.CanLoadFrom(null).Should().BeFalse();
        }

        [Test]
        public void CanLoadFrom_TypeMatch_IsTrue()
        {
            var loader = new TvDbEpisodeToAniDb(_sources, _mappingList, _episodeMapper);

            loader.CanLoadFrom(Substitute.For<ISourceData<TvDbEpisodeData>>()).Should().BeTrue();
        }

        [Test]
        public void CanLoadFrom_TypeMisMatch_IsFalse()
        {
            var loader = new TvDbEpisodeToAniDb(_sources, _mappingList, _episodeMapper);

            loader.CanLoadFrom(new object()).Should().BeFalse();
        }

        [Test]
        public async Task LoadFrom_NoTvDbSeriesData_Fails()
        {
            _sources.TvDb.GetSeriesData(_embyData, Arg.Any<ProcessResultContext>())
                .Returns(new ProcessFailedResult("", "", MediaItemTypes.Episode, "Failed"));

            var loader = new TvDbEpisodeToAniDb(_sources, _mappingList, _episodeMapper);

            var result = await loader.LoadFrom(_mediaItem, _tvDbSourceData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed"));
        }

        [Test]
        public async Task LoadFrom_NoAniDbSeriesId_Fails()
        {
            SetUpTvDbSeriesData(53);

            var loader = new TvDbEpisodeToAniDb(_sources, _mappingList, _episodeMapper);

            var result = await loader.LoadFrom(_mediaItem, _tvDbSourceData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to find AniDb series Id"));
        }

        [Test]
        public async Task LoadFrom_NoSeriesMapping_Fails()
        {
            SetUpTvDbSeriesData(53);
            _embyData.GetParentId(MediaItemTypes.Series, _sources.AniDb).Returns(19);

            var loader = new TvDbEpisodeToAniDb(_sources, _mappingList, _episodeMapper);

            var result = await loader.LoadFrom(_mediaItem, _tvDbSourceData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Should().Be(_noMappingResult));
        }

        [Test]
        public async Task LoadFrom_NoMatchingSeriesMappings_Fails()
        {
            SetUpTvDbSeriesData(53);
            _embyData.GetParentId(MediaItemTypes.Series, _sources.AniDb).Returns(19);

            var seriesMappings = new[]
            {
                CreateSeriesMapping(53, 55)
            };

            _mappingList.GetSeriesMappingsFromTvDb(53, Arg.Any<ProcessResultContext>()).Returns(seriesMappings);

            var loader = new TvDbEpisodeToAniDb(_sources, _mappingList, _episodeMapper);

            var result = await loader.LoadFrom(_mediaItem, _tvDbSourceData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("No series mapping between TvDb series Id '53' and AniDb series id '19'"));
        }

        [Test]
        public async Task LoadFrom_MultipleSeriesMappings_Fails()
        {
            SetUpTvDbSeriesData(53);
            _embyData.GetParentId(MediaItemTypes.Series, _sources.AniDb).Returns(19);

            var seriesMappings = new[]
            {
                CreateSeriesMapping(53, 19),
                CreateSeriesMapping(53, 19)
            };

            _mappingList.GetSeriesMappingsFromTvDb(53, Arg.Any<ProcessResultContext>()).Returns(seriesMappings);

            var loader = new TvDbEpisodeToAniDb(_sources, _mappingList, _episodeMapper);

            var result = await loader.LoadFrom(_mediaItem, _tvDbSourceData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Multiple series mappings found between TvDb series Id '53' and AniDb series Id '19'"));
        }

        [Test]
        public async Task LoadFrom_NoCorrespondingAniDbEpisode_Fails()
        {
            SetUpTvDbSeriesData(53);
            _embyData.GetParentId(MediaItemTypes.Series, _sources.AniDb).Returns(19);

            var seriesMapping = CreateSeriesMapping(53, 19);

            _mappingList.GetSeriesMappingsFromTvDb(53, Arg.Any<ProcessResultContext>()).Returns(new[]
            {
                seriesMapping
            });

            var loader = new TvDbEpisodeToAniDb(_sources, _mappingList, _episodeMapper);

            var result = await loader.LoadFrom(_mediaItem, _tvDbSourceData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to find a corresponding AniDb episode in AniDb series id '19'"));
        }

        [Test]
        public async Task LoadFrom_NoSelectableTitle_Fails()
        {
            SetUpTvDbSeriesData(53);
            _embyData.GetParentId(MediaItemTypes.Series, _sources.AniDb).Returns(19);

            var seriesMapping = CreateSeriesMapping(53, 19);

            _mappingList.GetSeriesMappingsFromTvDb(53, Arg.Any<ProcessResultContext>()).Returns(new[]
            {
                seriesMapping
            });

            var aniDbEpisodeData =  SetUpEpisodeMapping(67, 92, seriesMapping, "");

            _sources.AniDb.SelectTitle(aniDbEpisodeData.Titles, "en", Arg.Any<ProcessResultContext>())
                .Returns(new ProcessFailedResult("", "", MediaItemTypes.Episode, "Failed to find a title"));

            var loader = new TvDbEpisodeToAniDb(_sources, _mappingList, _episodeMapper);

            var result = await loader.LoadFrom(_mediaItem, _tvDbSourceData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to find a title"));
        }

        [Test]
        public async Task LoadFrom_ReturnsSourceDataWithSelectedTitle()
        {
            SetUpTvDbSeriesData(53);
            _embyData.GetParentId(MediaItemTypes.Series, _sources.AniDb).Returns(19);

            var seriesMapping = CreateSeriesMapping(53, 19);

            _mappingList.GetSeriesMappingsFromTvDb(53, Arg.Any<ProcessResultContext>()).Returns(new[]
            {
                seriesMapping
            });

            var aniDbEpisodeData = SetUpEpisodeMapping(67, 92, seriesMapping, "Title");
            
            var loader = new TvDbEpisodeToAniDb(_sources, _mappingList, _episodeMapper);

            var result = await loader.LoadFrom(_mediaItem, _tvDbSourceData);

            result.IsRight.Should().BeTrue();
            result.IfRight(sd => sd.Data.Should().Be(aniDbEpisodeData));
            result.IfRight(sd => sd.Source.Should().Be(_sources.AniDb));
            result.IfRight(sd => sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(92, 1, "Title")));
        }
    }
}
