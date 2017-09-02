using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Functional.Maybe;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.Files;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests.IntegrationTests
{
    [TestFixture]
    [Explicit]
    public class TvDbClientIntegrationTests
    {
        [SetUp]
        public void Setup()
        {
            _logManager = new ConsoleLogManager();
            _applicationPaths = Substitute.For<IApplicationPaths>();
            _fileCache = Substitute.For<IFileCache>();

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new MaybeJsonConverter() }
            };
        }

        private ConsoleLogManager _logManager;
        private IApplicationPaths _applicationPaths;
        private IFileCache _fileCache;

        [Test]
        public async Task GetEpisodesAsync_ValidSeriesId_ReturnsEpisodes()
        {
            var client = new TvDbClient(new TvDbConnection(new TestHttpClient(), new JsonSerialiser(), _logManager),
                _fileCache, _applicationPaths, _logManager, new JsonSerialiser(), new PluginConfiguration
                {
                    TvDbApiKey = Secrets.Instance.TvDbApiKey
                });

            var episodesResult = await client.GetEpisodesAsync(80675);

            episodesResult.HasValue.Should().BeTrue();
            var episodes = episodesResult.Value.ToList();

            episodes.Should().HaveCount(57);

            episodes[0]
                .ShouldBeEquivalentTo(new TvDbEpisodeData(340368, "Celestial Being", 1L.ToMaybe(), 1, 1, 1496255818));
        }
    }
}