using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.AniDb.Titles;
using MediaBrowser.Plugins.AniMetadata.Mapping;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using MediaBrowser.Plugins.AniMetadata.Providers;
using MediaBrowser.Plugins.AniMetadata.Tests.TestData;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests.Process.Sources
{
    [TestFixture]
    internal class TvDbSourceTests
    {
        [TestFixture]
        public class LookupFromOtherSourcesAsync : TvDbSourceTests
        {
            [SetUp]
            public virtual void Setup()
            {
                EmbyData = Substitute.For<IEmbyItemData>();
                EmbyData.Identifier.Returns(new ItemIdentifier(0, 0, "Name"));

                TvDbSeriesData = TvDbTestData.Series(30);
                AniDbSeriesData = new AniDbSeriesData();

                TvDbClient = Substitute.For<ITvDbClient>();
                TvDbClient.GetSeriesAsync(30).Returns(TvDbSeriesData);

                AniDbEpisodeData = new AniDbEpisodeData();

                SeriesMapping = Substitute.For<ISeriesMapping>();
                SeriesMapping.Ids.Returns(new SeriesIds(3, 30, Option<int>.None, Option<int>.None));

                MappingList = Substitute.For<IMappingList>();
                MappingList.GetSeriesMappingFromAniDb(3).Returns(Option<ISeriesMapping>.Some(SeriesMapping));

                AnimeMappingListFactory = Substitute.For<IAnimeMappingListFactory>();
                AnimeMappingListFactory.CreateMappingListAsync(CancellationToken.None)
                    .Returns(Option<IMappingList>.Some(MappingList));

                Sources = Substitute.For<ISources>();

                DataMapperFactory = Substitute.For<IDataMapperFactory>();
                DataMapper = Substitute.For<IDataMapper>();
                DataMapperFactory.GetDataMapperAsync().Returns(OptionAsync<IDataMapper>.Some(DataMapper));

                AniDbClient = Substitute.For<IAniDbClient>();
                AniDbClient.GetSeriesAsync(3).Returns(AniDbSeriesData);

                AniDbSource = Substitute.For<ISource>();
                Sources.AniDb.Returns(AniDbSource);

                AniDbSourceData = Substitute.For<ISourceData>();
                AniDbSourceData.Id.Returns(Option<int>.Some(3));
                AniDbSourceData.GetData<AniDbEpisodeData>().Returns(AniDbEpisodeData);

                MediaItem = Substitute.For<IMediaItem>();
                MediaItem.EmbyData.Returns(EmbyData);
                MediaItem.GetDataFromSource(AniDbSource).Returns(Option<ISourceData>.Some(AniDbSourceData));

                TvDbSource = new TvDbSource(TvDbClient, AnimeMappingListFactory, Sources, DataMapperFactory,
                    AniDbClient, new TitleNormaliser());
            }

            internal TvDbSeriesData TvDbSeriesData;
            internal AniDbSeriesData AniDbSeriesData;
            internal ITvDbClient TvDbClient;
            internal ISeriesMapping SeriesMapping;
            internal IMappingList MappingList;
            internal IAnimeMappingListFactory AnimeMappingListFactory;
            internal ISources Sources;
            internal IDataMapperFactory DataMapperFactory;
            internal IDataMapper DataMapper;
            internal IAniDbClient AniDbClient;
            internal IMediaItem MediaItem;
            internal ISource AniDbSource;
            internal TvDbSource TvDbSource;
            internal ISourceData AniDbSourceData;
            internal IEmbyItemData EmbyData;
            internal AniDbEpisodeData AniDbEpisodeData;

            [TestFixture]
            public class Series : LookupFromOtherSourcesAsync
            {
                [SetUp]
                public override void Setup()
                {
                    base.Setup();

                    MediaItem.ItemType.Returns(MediaItemTypes.Series);
                }

                [Test]
                public async Task FailedToLoadTvDbData_ReturnsFailedResult()
                {
                    TvDbClient.ClearSubstitute();

                    var result = await TvDbSource.LookupFromOtherSourcesAsync(MediaItem);

                    result.IsLeft.Should().BeTrue();
                    result.IfLeft(r => r.Reason.Should().Be("Failed to load TvDb series data"));
                }

                [Test]
                public async Task HasTvDbSeriesData_ReturnsSourceData()
                {
                    var result = await TvDbSource.LookupFromOtherSourcesAsync(MediaItem);

                    result.IsRight.Should().BeTrue();
                    result.IfRight(r =>
                    {
                        r.Id.ValueUnsafe().Should().Be(30);
                        r.Source.Should().Be(TvDbSource);
                        r.Identifier.Name.Should().Be(TvDbSeriesData.SeriesName);
                        r.Identifier.Index.IsNone.Should().BeTrue();
                        r.Identifier.ParentIndex.IsNone.Should().BeTrue();
                        r.GetData<TvDbSeriesData>().ValueUnsafe().Should().Be(TvDbSeriesData);
                    });
                }

                [Test]
                public async Task NoAniDbData_ReturnsFailedResult()
                {
                    MediaItem.ClearSubstitute();

                    var result = await TvDbSource.LookupFromOtherSourcesAsync(MediaItem);

                    result.IsLeft.Should().BeTrue();
                    result.IfLeft(r => r.Reason.Should().Be("No AniDb data set on this media item"));
                }

                [Test]
                public async Task NoAniDbId_ReturnsFailedResult()
                {
                    AniDbSourceData.ClearSubstitute();

                    var result = await TvDbSource.LookupFromOtherSourcesAsync(MediaItem);

                    result.IsLeft.Should().BeTrue();
                    result.IfLeft(r =>
                        r.Reason.Should().Be("No AniDb Id found on the AniDb data associated with this media item"));
                }

                [Test]
                public async Task NoMappingList_ReturnsFailedResult()
                {
                    AnimeMappingListFactory.ClearSubstitute();

                    var result = await TvDbSource.LookupFromOtherSourcesAsync(MediaItem);

                    result.IsLeft.Should().BeTrue();
                    result.IfLeft(r => r.Reason.Should().Be("No mapping found for AniDb series Id '3'"));
                }

                [Test]
                public async Task NoSeriesMapping_ReturnsFailedResult()
                {
                    MappingList.ClearSubstitute();

                    var result = await TvDbSource.LookupFromOtherSourcesAsync(MediaItem);

                    result.IsLeft.Should().BeTrue();
                    result.IfLeft(r => r.Reason.Should().Be("No mapping found for AniDb series Id '3'"));
                }

                [Test]
                public async Task NoTvDbIdOnSeriesMapping_ReturnsFailedResult()
                {
                    SeriesMapping.Ids.Returns(new SeriesIds(3, Option<int>.None, Option<int>.None, Option<int>.None));

                    var result = await TvDbSource.LookupFromOtherSourcesAsync(MediaItem);

                    result.IsLeft.Should().BeTrue();
                    result.IfLeft(r => r.Reason.Should().Be("No TvDb Id found on matching mapping"));
                }
            }

            [TestFixture]
            public class Season : LookupFromOtherSourcesAsync
            {
                [SetUp]
                public override void Setup()
                {
                    base.Setup();

                    MediaItem.ItemType.Returns(MediaItemTypes.Season);
                }

                [Test]
                public async Task ReturnsFailedResult()
                {
                    var result = await TvDbSource.LookupFromOtherSourcesAsync(MediaItem);

                    result.IsLeft.Should().BeTrue();
                    result.IfLeft(r =>
                        r.Reason.Should().Be("TvDb source cannot load season data by mapping from other sources"));
                }
            }

            [TestFixture]
            public class Episode : LookupFromOtherSourcesAsync
            {
                [SetUp]
                public override void Setup()
                {
                    base.Setup();

                    MediaItem.ItemType.Returns(MediaItemTypes.Episode);

                    EmbyData.GetParentId(MediaItemTypes.Series, AniDbSource).Returns(Option<int>.Some(3));

                    AniDbSourceData.GetData<SourceData<AniDbEpisodeData>>()
                        .Returns(new SourceData<AniDbEpisodeData>(AniDbSource, 3,
                            new ItemIdentifier(3, Option<int>.None, "episodeName"), AniDbEpisodeData));
                }


                [Test]
                public async Task DataMapperReturnsAniDbOnly_ReturnsFailedResult()
                {
                    DataMapper.MapEpisodeDataAsync(AniDbSeriesData, AniDbEpisodeData)
                        .Returns(new AniDbOnlyEpisodeData(AniDbEpisodeData));

                    var result = await TvDbSource.LookupFromOtherSourcesAsync(MediaItem);

                    result.IsLeft.Should().BeTrue();
                    result.IfLeft(r => r.Reason.Should().Be("Failed to find a corresponding TvDb episode"));
                }

                [Test]
                public async Task DataMapperReturnsCombined_ReturnsSourceData()
                {
                    var tvDbEpisodeData = TvDbTestData.Episode(30, 1, 2, 44, "episodeName");

                    DataMapper.MapEpisodeDataAsync(AniDbSeriesData, AniDbEpisodeData)
                        .Returns(new CombinedEpisodeData(AniDbEpisodeData, tvDbEpisodeData, new NoEpisodeData()));

                    var result = await TvDbSource.LookupFromOtherSourcesAsync(MediaItem);

                    result.IsRight.Should().BeTrue();
                    result.IfRight(r =>
                    {
                        r.Identifier.Name.Should().Be("episodeName");
                        r.Identifier.Index.ValueUnsafe().Should().Be(1);
                        r.Identifier.ParentIndex.ValueUnsafe().Should().Be(2);
                        r.Id.ValueUnsafe().Should().Be(30);
                        r.Source.Should().Be(TvDbSource);
                        r.GetData<TvDbEpisodeData>().ValueUnsafe().Should().Be(tvDbEpisodeData);
                    });
                }

                [Test]
                public async Task DataMapperReturnsNone_ReturnsFailedResult()
                {
                    DataMapper.MapEpisodeDataAsync(AniDbSeriesData, AniDbEpisodeData)
                        .Returns(new NoEpisodeData());

                    var result = await TvDbSource.LookupFromOtherSourcesAsync(MediaItem);

                    result.IsLeft.Should().BeTrue();
                    result.IfLeft(r => r.Reason.Should().Be("Failed to find a corresponding TvDb episode"));
                }

                [Test]
                public async Task FailedToLoadAniDbSeries_ReturnsFailedResult()
                {
                    AniDbClient.ClearSubstitute();

                    var result = await TvDbSource.LookupFromOtherSourcesAsync(MediaItem);

                    result.IsLeft.Should().BeTrue();
                    result.IfLeft(r => r.Reason.Should().Be("Failed to load parent series with AniDb Id '3'"));
                }

                [Test]
                public async Task NoAniDbData_ReturnsFailedResult()
                {
                    MediaItem.ClearSubstitute();

                    var result = await TvDbSource.LookupFromOtherSourcesAsync(MediaItem);

                    result.IsLeft.Should().BeTrue();
                    result.IfLeft(r => r.Reason.Should().Be("No AniDb data set on this media item"));
                }

                [Test]
                public async Task NoAniDbIdOnParentSeries_ReturnsFailedResult()
                {
                    EmbyData.GetParentId(MediaItemTypes.Series, AniDbSource).Returns(Option<int>.None);

                    var result = await TvDbSource.LookupFromOtherSourcesAsync(MediaItem);

                    result.IsLeft.Should().BeTrue();
                    result.IfLeft(r => r.Reason.Should().Be("No AniDb Id found on parent series"));
                }

                [Test]
                public async Task NoAniDbSourceData_ReturnsFailedResult()
                {
                    AniDbSourceData.ClearSubstitute();

                    var result = await TvDbSource.LookupFromOtherSourcesAsync(MediaItem);

                    result.IsLeft.Should().BeTrue();
                    result.IfLeft(r => r.Reason.Should().Be("No AniDb episode data associated with this media item"));
                }

                [Test]
                public async Task NoDataMapper_ReturnsFailedResult()
                {
                    DataMapperFactory.ClearSubstitute();

                    var result = await TvDbSource.LookupFromOtherSourcesAsync(MediaItem);

                    result.IsLeft.Should().BeTrue();
                    result.IfLeft(r => r.Reason.Should().Be("Data mapper could not be created"));
                }
            }
        }
    }
}