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
using MediaBrowser.Plugins.Anime.AniDb.Titles;
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

        private EpisodeInfo EpisodeInfoS01E03 => new EpisodeInfo
        {
            Name = "EpisodeName",
            SeriesProviderIds = new Dictionary<string, string> { { "AniDB", "324" } },
            ParentIndexNumber = 1,
            IndexNumber = 3,
            ProviderIds = new Dictionary<string, string>()
        };

        [Test]
        public async Task GetMetadata_NoMatchingEpisode_ReturnsBlankMetadata()
        {
            _aniDbClient.GetSeriesAsync("324")
                .Returns(Task.FromResult(new AniDbSeriesData().WithStandardData().ToMaybe()));

            var episodeProvider = new AniDbEpisodeProvider(_aniDbClient, _metadataFactory, _logManager,
                new TitleNormaliser());

            var metadata = await episodeProvider.GetMetadata(EpisodeInfoS01E03, CancellationToken.None);

            metadata.Item.Should().BeNull();
        }

        [Test]
        public async Task GetMetadata_NoSeasonProvided_MatchesOnTitle()
        {
            var series = new AniDbSeriesData().WithStandardData();
            series.Episodes = new[]
            {
                new EpisodeData
                {
                    Id = 442,
                    RawEpisodeNumber = new EpisodeNumberData
                    {
                        RawNumber = "55",
                        RawType = 1
                    },
                    Titles = new[]
                    {
                        new EpisodeTitleData
                        {
                            Language = "en",
                            Title = "EpisodeTitle",
                            Type = "Official"
                        }
                    }
                }
            };

            _aniDbClient.GetSeriesAsync("324")
                .Returns(Task.FromResult(series.ToMaybe()));

            var episodeProvider = new AniDbEpisodeProvider(_aniDbClient, _metadataFactory, _logManager,
                new TitleNormaliser());

            var episodeInfo = EpisodeInfoS01E03;
            episodeInfo.ParentIndexNumber = null;
            episodeInfo.Name = "EpisodeTitle";

            var metadata = await episodeProvider.GetMetadata(episodeInfo, CancellationToken.None);

            _metadataFactory.Received(1).CreateEpisodeMetadataResult(series.Episodes[0], null, null);
        }
    }
}