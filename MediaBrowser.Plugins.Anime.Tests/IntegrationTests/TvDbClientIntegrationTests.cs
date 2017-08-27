using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MediaBrowser.Plugins.Anime.Tests.TestHelpers;
using MediaBrowser.Plugins.Anime.TvDb;
using MediaBrowser.Plugins.Anime.TvDb.Data;
using NUnit.Framework;

namespace MediaBrowser.Plugins.Anime.Tests.IntegrationTests
{
    [TestFixture]
    [Explicit]
    public class TvDbClientIntegrationTests
    {
        [SetUp]
        public void Setup()
        {
            _logManager = new ConsoleLogManager();
        }

        private ConsoleLogManager _logManager;

        [Test]
        public async Task GetEpisodesAsync_ValidSeriesId_ReturnsEpisodes()
        {
            var client = new TvDbClient(new TvDbConnection(new TestHttpClient(), new JsonSerialiser(), _logManager),
                _logManager);

            var episodesResult = await client.GetEpisodesAsync(80675);

            episodesResult.HasValue.Should().BeTrue();
            var episodes = episodesResult.Value.ToList();

            episodes.Should().HaveCount(57);

            episodes[0].ShouldBeEquivalentTo(new TvDbEpisodeData(340368, "Celestial Being", 1, 1, 1, 1496255818));
        }
    }
}