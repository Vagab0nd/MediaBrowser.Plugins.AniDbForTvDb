using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Functional.Maybe;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.Anime.AniDb;
using MediaBrowser.Plugins.Anime.AniDb.Data;
using MediaBrowser.Plugins.Anime.Providers.AniDb2;
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

            _metadataFactory.NullEpisodeResult.Returns(new MetadataResult<Episode>());
        }

        private IAniDbClient _aniDbClient;
        private IEmbyMetadataFactory _metadataFactory;
        private ILogManager _logManager;

        private AniDbSeries TestSeries => new AniDbSeries
        {
            Id = 324
        };

        private EpisodeInfo EpisodeInfoS01E03 => new EpisodeInfo
        {
            Name = "EpisodeName",
            SeriesProviderIds = new Dictionary<string, string> { { "AniDB", "324" } },
            ParentIndexNumber = 1,
            IndexNumber = 3
        };

        [Test]
        public async Task GetMetadata_NoMatchingEpisode_ReturnsBlankMetadata()
        {
            _aniDbClient.GetSeriesAsync("324").Returns(Task.FromResult(TestSeries.ToMaybe()));

            var episodeProvider = new AniDbEpisodeProvider(_aniDbClient, _metadataFactory, _logManager);

            var metadata = await episodeProvider.GetMetadata(EpisodeInfoS01E03, CancellationToken.None);

            metadata.Item.Should().BeNull();
        }
    }
}