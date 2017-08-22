using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Functional.Maybe;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.Anime.AniDb;
using MediaBrowser.Plugins.Anime.AniDb.Mapping;
using MediaBrowser.Plugins.Anime.AniDb.Series.Data;
using MediaBrowser.Plugins.Anime.Providers.AniDb2;
using MediaBrowser.Plugins.Anime.Tests.TestData;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.Anime.Tests
{
    [TestFixture]
    public class AniDbEpisodeProviderTests
    {
        [SetUp]
        public void Setup()
        {
            _aniDbClient = Substitute.For<IAniDbClient>();
            _metadataFactory = Substitute.For<IEmbyMetadataFactory>();
            _logManager = Substitute.For<ILogManager>();
            _episodeMatcher = Substitute.For<IEpisodeMatcher>();
            _mapper = Substitute.For<IAniDbMapper>();

            _aniDbClient.GetMapperAsync().Returns(_mapper.ToMaybe());
            _metadataFactory.NullEpisodeResult.Returns(new MetadataResult<Episode>());
            _metadataFactory.CreateEpisodeMetadataResult(null, null, null)
                .ReturnsForAnyArgs(new MetadataResult<Episode>());
        }

        private IAniDbClient _aniDbClient;
        private IEmbyMetadataFactory _metadataFactory;
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
        public async Task GetMetadata_MatchingEpisode_GetsMapper()
        {
            _aniDbClient.GetSeriesAsync("324")
                .Returns(Task.FromResult(new AniDbSeriesData().WithStandardData().ToMaybe()));

            _episodeMatcher.FindEpisode(null, Maybe<int>.Nothing, Maybe<int>.Nothing, Maybe<string>.Nothing)
                .ReturnsForAnyArgs(new EpisodeData().ToMaybe());

            var episodeProvider =
                new AniDbEpisodeProvider(_aniDbClient, _metadataFactory, _logManager, _episodeMatcher);

            await episodeProvider.GetMetadata(EpisodeInfoS01E03, CancellationToken.None);

            _aniDbClient.Received(1).GetMapperAsync();
        }

        [Test]
        public async Task GetMetadata_MatchingSeries_FindsEpisode()
        {
            _aniDbClient.GetSeriesAsync("324")
                .Returns(Task.FromResult(new AniDbSeriesData().WithStandardData().ToMaybe()));

            var episodeProvider =
                new AniDbEpisodeProvider(_aniDbClient, _metadataFactory, _logManager, _episodeMatcher);

            await episodeProvider.GetMetadata(EpisodeInfoS01E03, CancellationToken.None);

            _episodeMatcher.ReceivedWithAnyArgs(1)
                .FindEpisode(null, Maybe<int>.Nothing, Maybe<int>.Nothing, Maybe<string>.Nothing);
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
            var episodes = new[] { new EpisodeData(), new EpisodeData() };
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

            _aniDbClient.GetSeriesAsync("324").Returns(Task.FromResult(series.ToMaybe()));

            var episodeProvider =
                new AniDbEpisodeProvider(_aniDbClient, _metadataFactory, _logManager, _episodeMatcher);

            await episodeProvider.GetMetadata(episodeInfo, CancellationToken.None);

            _episodeMatcher.ReceivedWithAnyArgs(1)
                .FindEpisode(episodes, seasonIndex.ToMaybe(), episodeIndex.ToMaybe(), title.ToMaybe());
        }

        [Test]
        public async Task GetMetadata_NoMatchingEpisode_ReturnsBlankMetadata()
        {
            _aniDbClient.GetSeriesAsync("324")
                .Returns(Task.FromResult(new AniDbSeriesData().WithStandardData().ToMaybe()));

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