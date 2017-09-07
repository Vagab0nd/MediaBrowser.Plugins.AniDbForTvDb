using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Providers.AniDb;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class AniDbSeriesProviderTests
    {
        [SetUp]
        public void Setup()
        {
            _logManager = new ConsoleLogManager();
            _aniDbClient = Substitute.For<IAniDbClient>();
            _embyMetadataFactory = Substitute.For<IEmbyMetadataFactory>();
        }

        private ILogManager _logManager;
        private IAniDbClient _aniDbClient;
        private IEmbyMetadataFactory _embyMetadataFactory;

        [Test]
        public async Task GetMetadata_ReturnsCreatedMetadataResult()
        {
            var aniDbSeriesProvider =
                new AniDbSeriesProvider(_logManager, _aniDbClient, _embyMetadataFactory);

            var seriesInfo = new SeriesInfo
            {
                Name = "AniDbTitle",
                MetadataLanguage = "en"
            };

            var aniDbSeriesData = new AniDbSeriesData();

            var expectedResult = new MetadataResult<Series>();

            _aniDbClient.FindSeriesAsync("AniDbTitle").Returns(aniDbSeriesData);
            _embyMetadataFactory.CreateSeriesMetadataResult(aniDbSeriesData, "en").Returns(expectedResult);

            var result = await aniDbSeriesProvider.GetMetadata(seriesInfo, CancellationToken.None);

            result.Should().Be(expectedResult);
        }
    }
}