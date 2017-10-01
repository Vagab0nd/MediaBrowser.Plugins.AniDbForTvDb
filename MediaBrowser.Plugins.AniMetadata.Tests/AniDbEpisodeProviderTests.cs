using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Providers.AniDb;
using MediaBrowser.Plugins.AniMetadata.Tests.TestData;
using NSubstitute;
using NUnit.Framework;
using static LanguageExt.Prelude;

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
            _mapper = Substitute.For<IAniDbMapper>();

            _metadataFactory.NullResult.Returns(new MetadataResult<Episode>());
            _metadataFactory.CreateMetadata(null, null)
                .ReturnsForAnyArgs(new MetadataResult<Episode>());
            _metadataFactory.CreateMetadata(null, null, null)
                .ReturnsForAnyArgs(new MetadataResult<Episode>());
        }

        private IAniDbClient _aniDbClient;
        private IEpisodeMetadataFactory _metadataFactory;
        private ILogManager _logManager;
        private IAniDbMapper _mapper;
        private IEpisodeMatcher _episodeMatcher;

        private EpisodeInfo EpisodeInfoS01E03 => new EpisodeInfo
        {
            Name = "EpisodeName",
            SeriesProviderIds = new Dictionary<string, string> { { "AniDB", "324" } },
            ParentIndexNumber = 1,
            IndexNumber = 3,
            ProviderIds = new Dictionary<string, string>()
        };

        [Test]
        public async Task GetMetadata_HasMapper_CreatesMetadataResult()
        {
            var episodeData = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "5",
                    RawType = 1
                }
            };

            var mappedEpisodeResult =
                (MappedEpisodeResult)new TvDbEpisodeNumber(Option<int>.None, 1, 5,
                    Option<TvDbEpisodeNumber>.None);

            _aniDbClient.GetSeriesAsync("324")
                .Returns(new AniDbSeriesData().WithStandardData());

            _aniDbClient.GetMapperAsync().Returns(Option<IAniDbMapper>.Some(_mapper));

            _episodeMatcher.FindEpisode(null, Option<int>.None, Option<int>.None, Option<string>.None)
                .ReturnsForAnyArgs(episodeData);

            _mapper.GetMappedTvDbEpisodeIdAsync(324, episodeData.EpisodeNumber)
                .Returns(mappedEpisodeResult);

            var episodeProvider =
                new AniDbEpisodeProvider(_aniDbClient, _metadataFactory, _logManager, _episodeMatcher);

            await episodeProvider.GetMetadata(EpisodeInfoS01E03, CancellationToken.None);

            _metadataFactory.Received(1).CreateMetadata(episodeData, mappedEpisodeResult);
        }

        [Test]
        public async Task GetMetadata_HasMapper_ReturnsCreatedMetadataResult()
        {
            var episodeData = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "5",
                    RawType = 1
                }
            };

            var mappedEpisodeResult =
                (MappedEpisodeResult)new TvDbEpisodeNumber(Option<int>.None, 1, 5,
                    Option<TvDbEpisodeNumber>.None);

            var metadataResult = new MetadataResult<Episode>
            {
                Item = new Episode()
            };

            _aniDbClient.GetSeriesAsync("324")
                .Returns(Task.FromResult(Some(new AniDbSeriesData().WithStandardData())));

            _aniDbClient.GetMapperAsync().Returns(Option<IAniDbMapper>.Some(_mapper));

            _episodeMatcher.FindEpisode(null, Option<int>.None, Option<int>.None, Option<string>.None)
                .ReturnsForAnyArgs(episodeData);

            _mapper.GetMappedTvDbEpisodeIdAsync(324, episodeData.EpisodeNumber)
                .Returns(mappedEpisodeResult);

            _metadataFactory.CreateMetadata(episodeData, mappedEpisodeResult)
                .Returns(metadataResult);

            var episodeProvider =
                new AniDbEpisodeProvider(_aniDbClient, _metadataFactory, _logManager, _episodeMatcher);

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
                new AniDbEpisodeProvider(_aniDbClient, _metadataFactory, _logManager, _episodeMatcher);

            await episodeProvider.GetMetadata(EpisodeInfoS01E03, CancellationToken.None);

            _aniDbClient.Received(1).GetMapperAsync();
        }

        [Test]
        public async Task GetMetadata_MatchingSeries_FindsEpisode()
        {
            _aniDbClient.GetSeriesAsync("324")
                .Returns(new AniDbSeriesData().WithStandardData());

            var episodeProvider =
                new AniDbEpisodeProvider(_aniDbClient, _metadataFactory, _logManager, _episodeMatcher);

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
                new AniDbEpisodeProvider(_aniDbClient, _metadataFactory, _logManager, _episodeMatcher);

            await episodeProvider.GetMetadata(episodeInfo, CancellationToken.None);

            _episodeMatcher.ReceivedWithAnyArgs(1)
                .FindEpisode(episodes, seasonIndex.ToOption(), episodeIndex.ToOption(), title);
        }

        [Test]
        public async Task GetMetadata_NoMapper_ReturnsBlankMetadata()
        {
            _aniDbClient.GetSeriesAsync("324")
                .Returns(new AniDbSeriesData().WithStandardData());

            _aniDbClient.GetMapperAsync().Returns(Option<IAniDbMapper>.None);

            _episodeMatcher.FindEpisode(null, Option<int>.None, Option<int>.None, Option<string>.None)
                .ReturnsForAnyArgs(new AniDbEpisodeData());

            var episodeProvider =
                new AniDbEpisodeProvider(_aniDbClient, _metadataFactory, _logManager, _episodeMatcher);

            var metadata = await episodeProvider.GetMetadata(EpisodeInfoS01E03, CancellationToken.None);

            metadata.Item.Should().BeNull();
        }

        [Test]
        public async Task GetMetadata_NoMatchingEpisode_ReturnsBlankMetadata()
        {
            _aniDbClient.GetSeriesAsync("324")
                .Returns(new AniDbSeriesData().WithStandardData());

            var episodeProvider =
                new AniDbEpisodeProvider(_aniDbClient, _metadataFactory, _logManager, _episodeMatcher);

            var metadata = await episodeProvider.GetMetadata(EpisodeInfoS01E03, CancellationToken.None);

            metadata.Item.Should().BeNull();
        }

        [Test]
        public async Task GetMetadata_NoMatchingSeries_ReturnsBlankMetadata()
        {
            var episodeProvider =
                new AniDbEpisodeProvider(_aniDbClient, _metadataFactory, _logManager, _episodeMatcher);

            var metadata = await episodeProvider.GetMetadata(EpisodeInfoS01E03, CancellationToken.None);

            metadata.Item.Should().BeNull();
        }
    }
}