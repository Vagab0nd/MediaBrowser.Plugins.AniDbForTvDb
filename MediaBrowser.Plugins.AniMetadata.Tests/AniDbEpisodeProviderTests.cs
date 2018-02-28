using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Mapping;
using MediaBrowser.Plugins.AniMetadata.Providers;
using MediaBrowser.Plugins.AniMetadata.Providers.AniDb;
using MediaBrowser.Plugins.AniMetadata.Tests.TestData;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class AniDbEpisodeProviderTests
    {
        [SetUp]
        public void Setup()
        {
            _aniDbClient = Substitute.For<IAniDbClient>();
            _metadataFactory = Substitute.For<IEpisodeMetadataFactory>();
            _logManager = Substitute.For<ILogManager>();
            _episodeMatcher = Substitute.For<IEpisodeMatcher>();
            _mapper = Substitute.For<IDataMapper>();
            _dataMapperFactory = Substitute.For<IDataMapperFactory>();

            _metadataFactory.NullResult.Returns(new MetadataResult<Episode>());
            _metadataFactory.CreateMetadata(null, null)
                .ReturnsForAnyArgs(new MetadataResult<Episode>());
        }

        private IAniDbClient _aniDbClient;
        private IEpisodeMetadataFactory _metadataFactory;
        private ILogManager _logManager;
        private IDataMapper _mapper;
        private IEpisodeMatcher _episodeMatcher;
        private IDataMapperFactory _dataMapperFactory;

        private EpisodeInfo EpisodeInfoS01E03 => new EpisodeInfo
        {
            Name = "EpisodeName",
            SeriesProviderIds = new Dictionary<string, string> { { "AniDB", "324" } },
            ParentIndexNumber = 1,
            IndexNumber = 3,
            ProviderIds = new Dictionary<string, string>(),
            MetadataLanguage = "en"
        };

        [Test]
        public async Task GetMetadata_HasMapper_CreatesMetadataResult()
        {
            var aniDbEpisodeData = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "5",
                    RawType = 1
                }
            };

            var aniDbSeriesData = new AniDbSeriesData().WithStandardData();

            _aniDbClient.GetSeriesAsync("324")
                .Returns(aniDbSeriesData);

            _dataMapperFactory.GetDataMapperAsync().Returns(OptionAsync<IDataMapper>.Some(_mapper));

            _episodeMatcher.FindEpisode(null, Option<int>.None, Option<int>.None, Option<string>.None)
                .ReturnsForAnyArgs(aniDbEpisodeData);

            var episodeData = new AniDbOnlyEpisodeData(aniDbEpisodeData);

            _mapper.MapEpisodeDataAsync(aniDbSeriesData, aniDbEpisodeData)
                .Returns(episodeData);

            var episodeProvider =
                new AniDbEpisodeProvider(_aniDbClient, _metadataFactory, _logManager, _episodeMatcher, _dataMapperFactory);

            await episodeProvider.GetMetadata(EpisodeInfoS01E03, CancellationToken.None);

            _metadataFactory.Received(1).CreateMetadata(episodeData, "en");
        }

        [Test]
        public async Task GetMetadata_HasMapper_ReturnsCreatedMetadataResult()
        {
            var aniDbEpisodeData = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "5",
                    RawType = 1
                }
            };

            var metadataResult = new MetadataResult<Episode>
            {
                Item = new Episode()
            };

            var aniDbSeriesData = new AniDbSeriesData().WithStandardData();

            _aniDbClient.GetSeriesAsync("324").Returns(aniDbSeriesData);

            _dataMapperFactory.GetDataMapperAsync().Returns(OptionAsync<IDataMapper>.Some(_mapper));

            _episodeMatcher.FindEpisode(null, Option<int>.None, Option<int>.None, Option<string>.None)
                .ReturnsForAnyArgs(aniDbEpisodeData);

            var episodeData = new AniDbOnlyEpisodeData(aniDbEpisodeData);

            _mapper.MapEpisodeDataAsync(aniDbSeriesData, aniDbEpisodeData)
                .Returns(episodeData);

            _metadataFactory.CreateMetadata(episodeData, "en").Returns(metadataResult);

            var episodeProvider =
                new AniDbEpisodeProvider(_aniDbClient, _metadataFactory, _logManager, _episodeMatcher, _dataMapperFactory);

            var metadata = await episodeProvider.GetMetadata(EpisodeInfoS01E03, CancellationToken.None);

            metadata.Should().Be(metadataResult);
        }

        [Test]
        public async Task GetMetadata_MatchingEpisode_GetsMapper()
        {
            _aniDbClient.GetSeriesAsync("324")
                .Returns(new AniDbSeriesData().WithStandardData());

            _episodeMatcher.FindEpisode(null, Option<int>.None, Option<int>.None, Option<string>.None)
                .ReturnsForAnyArgs(new AniDbEpisodeData());

            var episodeProvider =
                new AniDbEpisodeProvider(_aniDbClient, _metadataFactory, _logManager, _episodeMatcher, _dataMapperFactory);

            await episodeProvider.GetMetadata(EpisodeInfoS01E03, CancellationToken.None);

            _dataMapperFactory.Received(1).GetDataMapperAsync();
        }

        [Test]
        public async Task GetMetadata_MatchingSeries_FindsEpisode()
        {
            _aniDbClient.GetSeriesAsync("324")
                .Returns(new AniDbSeriesData().WithStandardData());

            var episodeProvider =
                new AniDbEpisodeProvider(_aniDbClient, _metadataFactory, _logManager, _episodeMatcher, _dataMapperFactory);

            await episodeProvider.GetMetadata(EpisodeInfoS01E03, CancellationToken.None);

            _episodeMatcher.ReceivedWithAnyArgs(1)
                .FindEpisode(null, Option<int>.None, Option<int>.None, Option<string>.None);
        }

        [Test]
        [TestCase(null, null, null)]
        [TestCase(1, 2, "title")]
        [TestCase(1, null, null)]
        [TestCase(null, 2, null)]
        [TestCase(null, null, "title")]
        public async Task GetMetadata_MatchingSeries_PassesEpisodeInfoDataToEpisodeMatcher(int? seasonIndex,
            int? episodeIndex, string title)
        {
            var episodes = new[] { new AniDbEpisodeData(), new AniDbEpisodeData() };
            var series = new AniDbSeriesData().WithStandardData();

            series.Episodes = episodes;

            var episodeInfo = new EpisodeInfo
            {
                IndexNumber = episodeIndex,
                ParentIndexNumber = seasonIndex,
                Name = title,
                ProviderIds = new Dictionary<string, string>(),
                SeriesProviderIds = new Dictionary<string, string> { { "AniDB", "324" } }
            };

            _aniDbClient.GetSeriesAsync("324").Returns(series);

            var episodeProvider =
                new AniDbEpisodeProvider(_aniDbClient, _metadataFactory, _logManager, _episodeMatcher, _dataMapperFactory);

            await episodeProvider.GetMetadata(episodeInfo, CancellationToken.None);

            _episodeMatcher.ReceivedWithAnyArgs(1)
                .FindEpisode(episodes, seasonIndex.ToOption(), episodeIndex.ToOption(), title);
        }

        [Test]
        public async Task GetMetadata_NoMapper_ReturnsBlankMetadata()
        {
            _aniDbClient.GetSeriesAsync("324")
                .Returns(new AniDbSeriesData().WithStandardData());

            _dataMapperFactory.GetDataMapperAsync().Returns(OptionAsync<IDataMapper>.None);

            _episodeMatcher.FindEpisode(null, Option<int>.None, Option<int>.None, Option<string>.None)
                .ReturnsForAnyArgs(new AniDbEpisodeData());

            var episodeProvider =
                new AniDbEpisodeProvider(_aniDbClient, _metadataFactory, _logManager, _episodeMatcher, _dataMapperFactory);

            var metadata = await episodeProvider.GetMetadata(EpisodeInfoS01E03, CancellationToken.None);

            metadata.Item.Should().BeNull();
        }

        [Test]
        public async Task GetMetadata_NoMatchingEpisode_ReturnsBlankMetadata()
        {
            _aniDbClient.GetSeriesAsync("324")
                .Returns(new AniDbSeriesData().WithStandardData());

            var episodeProvider =
                new AniDbEpisodeProvider(_aniDbClient, _metadataFactory, _logManager, _episodeMatcher, _dataMapperFactory);

            var metadata = await episodeProvider.GetMetadata(EpisodeInfoS01E03, CancellationToken.None);

            metadata.Item.Should().BeNull();
        }

        [Test]
        public async Task GetMetadata_NoMatchingSeries_ReturnsBlankMetadata()
        {
            var episodeProvider =
                new AniDbEpisodeProvider(_aniDbClient, _metadataFactory, _logManager, _episodeMatcher, _dataMapperFactory);

            var metadata = await episodeProvider.GetMetadata(EpisodeInfoS01E03, CancellationToken.None);

            metadata.Item.Should().BeNull();
        }

        [Test]
        public async Task GetMetadata_NonBlankResult_StopsOtherProviders()
        {
            var aniDbEpisodeData = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "5",
                    RawType = 1
                }
            };

            var metadataResult = new MetadataResult<Episode>
            {
                Item = new Episode(),
                HasMetadata = true
            };

            var aniDbSeriesData = new AniDbSeriesData().WithStandardData();

            _aniDbClient.GetSeriesAsync("324")
                .Returns(aniDbSeriesData);

            _dataMapperFactory.GetDataMapperAsync().Returns(OptionAsync<IDataMapper>.Some(_mapper));

            _episodeMatcher.FindEpisode(null, Option<int>.None, Option<int>.None, Option<string>.None)
                .ReturnsForAnyArgs(aniDbEpisodeData);

            var episodeData = new AniDbOnlyEpisodeData(aniDbEpisodeData);

            _mapper.MapEpisodeDataAsync(aniDbSeriesData, aniDbEpisodeData)
                .Returns(episodeData);

            var episodeProvider =
                new AniDbEpisodeProvider(_aniDbClient, _metadataFactory, _logManager, _episodeMatcher, _dataMapperFactory);

            _metadataFactory.CreateMetadata(episodeData, "en").Returns(metadataResult);

            var info = EpisodeInfoS01E03;

            await episodeProvider.GetMetadata(info, CancellationToken.None);

            info.Name.Should().BeEmpty();
            info.IndexNumber.Should().BeNull();
            info.ParentIndexNumber.Should().BeNull();
            info.ProviderIds.Should().BeEmpty();
        }
    }
}