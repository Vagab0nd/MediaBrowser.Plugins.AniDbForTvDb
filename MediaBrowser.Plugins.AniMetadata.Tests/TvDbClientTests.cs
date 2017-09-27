using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.Files;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;
using MediaBrowser.Plugins.AniMetadata.TvDb.Requests;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class TvDbClientTests
    {
        [SetUp]
        public void Setup()
        {
            _tvDbConnection = Substitute.For<ITvDbConnection>();
            _fileCache = Substitute.For<IFileCache>();
            _applicationPaths = Substitute.For<IApplicationPaths>();
            _logManager = new ConsoleLogManager();

            _tvDbConnection.PostAsync(Arg.Any<LoginRequest>(), Option<string>.None)
                .ReturnsForAnyArgs(new Response<LoginRequest.Response>(new LoginRequest.Response("TOKEN")));
        }

        private ITvDbConnection _tvDbConnection;
        private IFileCache _fileCache;
        private IApplicationPaths _applicationPaths;
        private ConsoleLogManager _logManager;

        [Test]
        public async Task GetEpisodeAsync_NoLocalEpisodeData_RequestsEpisodeData()
        {
            var episode = new TvDbEpisodeData(1, "Test", 1L, 2, 3, 4);

            _tvDbConnection.GetAsync(Arg.Any<GetEpisodesRequest>(), Arg.Any<Option<string>>())
                .Returns(new Response<GetEpisodesRequest.Response>(new GetEpisodesRequest.Response(new[] { episode },
                        new GetEpisodesRequest.PageLinks(1, 1, Option<int>.None, Option<int>.None))));

            var tvDbClient = new TvDbClient(_tvDbConnection, _fileCache, _applicationPaths, _logManager,
                new JsonSerialiser(), new PluginConfiguration());

            await tvDbClient.GetEpisodesAsync(4);

            _tvDbConnection.ReceivedWithAnyArgs(1)
                .GetAsync<GetEpisodesRequest.Response>(null, Option<string>.None);
        }

        [Test]
        public async Task GetEpisodeAsync_NoLocalEpisodeData_ReturnsNewEpisodeData()
        {
            var episode = new TvDbEpisodeData(1, "Test", 1L, 2, 3, 4);

            _tvDbConnection.GetAsync(Arg.Any<GetEpisodesRequest>(), Arg.Any<Option<string>>())
                .Returns(new Response<GetEpisodesRequest.Response>(new GetEpisodesRequest.Response(new[] { episode },
                        new GetEpisodesRequest.PageLinks(1, 1, Option<int>.None, Option<int>.None))));

            var tvDbClient = new TvDbClient(_tvDbConnection, _fileCache, _applicationPaths, _logManager,
                new JsonSerialiser(), new PluginConfiguration());

            var episodes = await tvDbClient.GetEpisodesAsync(4);

            episodes.IsSome.Should().BeTrue();
            episodes.ValueUnsafe().Should().BeEquivalentTo(episode);
        }

        [Test]
        public async Task GetEpisodeAsync_NoLocalEpisodeData_SavesNewEpisodeData()
        {
            var episode = new TvDbEpisodeData(1, "Test", 1L, 2, 3, 4);

            _tvDbConnection.GetAsync(Arg.Any<GetEpisodesRequest>(), Arg.Any<Option<string>>())
                .Returns(new Response<GetEpisodesRequest.Response>(new GetEpisodesRequest.Response(new[] { episode },
                        new GetEpisodesRequest.PageLinks(1, 1, Option<int>.None, Option<int>.None))));

            var tvDbClient = new TvDbClient(_tvDbConnection, _fileCache, _applicationPaths, _logManager,
                new JsonSerialiser(), new PluginConfiguration());

            await tvDbClient.GetEpisodesAsync(4);

            _fileCache.Received(1)
                .SaveFile(Arg.Any<TvDbSeriesEpisodesFileSpec>(),
                    Arg.Is<TvDbEpisodeCollection>(d => d.Episodes.Single() == episode));
        }

        [Test]
        public async Task GetEpisodesAsync_FailedResponse_ReturnsNone()
        {
            _tvDbConnection.GetAsync(Arg.Any<GetEpisodesRequest>(), Arg.Any<Option<string>>())
                .Returns(new FailedRequest(HttpStatusCode.BadRequest, "Failed"));

            var tvDbClient = new TvDbClient(_tvDbConnection, _fileCache, _applicationPaths, _logManager,
                new JsonSerialiser(), new PluginConfiguration());

            var episodes = await tvDbClient.GetEpisodesAsync(4);

            episodes.IsSome.Should().BeFalse();
        }

        [Test]
        public async Task GetEpisodesAsync_LocalEpisodeData_DoesNotRequestEpisodeData()
        {
            var episode = new TvDbEpisodeData(1, "Test", 1L, 2, 3, 4);

            _fileCache.GetFileContent(Arg.Any<TvDbSeriesEpisodesFileSpec>())
                .Returns(new TvDbEpisodeCollection(new[] { episode }));

            var tvDbClient = new TvDbClient(_tvDbConnection, _fileCache, _applicationPaths, _logManager,
                new JsonSerialiser(), new PluginConfiguration());

            await tvDbClient.GetEpisodesAsync(4);

            _tvDbConnection.DidNotReceiveWithAnyArgs()
                .GetAsync<GetEpisodesRequest.Response>(null, Option<string>.None);
        }

        [Test]
        public async Task GetEpisodesAsync_LocalEpisodeData_ReturnsLocalEpisodeData()
        {
            var episode = new TvDbEpisodeData(1, "Test", 1L, 2, 3, 4);

            _fileCache.GetFileContent(Arg.Any<TvDbSeriesEpisodesFileSpec>())
                .Returns(new TvDbEpisodeCollection(new[] { episode }));

            var tvDbClient = new TvDbClient(_tvDbConnection, _fileCache, _applicationPaths, _logManager,
                new JsonSerialiser(), new PluginConfiguration());

            var episodes = await tvDbClient.GetEpisodesAsync(4);

            episodes.IsSome.Should().BeTrue();
            episodes.ValueUnsafe().Should().BeEquivalentTo(episode);
        }

        [Test]
        public async Task GetEpisodesAsync_MultiPageResponse_RequestsAllPages()
        {
            var page1Episode = new TvDbEpisodeData(1, "Test1", 1L, 2, 3, 4);
            var page2Episode = new TvDbEpisodeData(2, "Test2", 5L, 6, 7, 8);

            _tvDbConnection
                .GetAsync(Arg.Is<GetEpisodesRequest>(r => r.Url == "https://api.thetvdb.com/series/4/episodes?page=1"),
                    Arg.Any<Option<string>>())
                .Returns(new Response<GetEpisodesRequest.Response>(new GetEpisodesRequest.Response(new[] { page1Episode },
                        new GetEpisodesRequest.PageLinks(1, 2, 2, Option<int>.None))));

            _tvDbConnection
                .GetAsync(Arg.Is<GetEpisodesRequest>(r => r.Url == "https://api.thetvdb.com/series/4/episodes?page=2"),
                    Arg.Any<Option<string>>())
                .Returns(new Response<GetEpisodesRequest.Response>(new GetEpisodesRequest.Response(new[] { page2Episode },
                        new GetEpisodesRequest.PageLinks(2, 2, Option<int>.None, 1))));

            var tvDbClient = new TvDbClient(_tvDbConnection, _fileCache, _applicationPaths, _logManager,
                new JsonSerialiser(), new PluginConfiguration());

            await tvDbClient.GetEpisodesAsync(4);

            _tvDbConnection.Received(1)
                .GetAsync(Arg.Is<GetEpisodesRequest>(r => r.Url == "https://api.thetvdb.com/series/4/episodes?page=1"),
                    Arg.Any<Option<string>>());
            _tvDbConnection.Received(1)
                .GetAsync(Arg.Is<GetEpisodesRequest>(r => r.Url == "https://api.thetvdb.com/series/4/episodes?page=2"),
                    Arg.Any<Option<string>>());
        }

        [Test]
        public async Task GetEpisodesAsync_MultiPageResponse_ReturnsAllPagesConcatenated()
        {
            var page1Episode = new TvDbEpisodeData(1, "Test1", 1L, 2, 3, 4);
            var page2Episode = new TvDbEpisodeData(2, "Test2", 5L, 6, 7, 8);

            _tvDbConnection
                .GetAsync(Arg.Is<GetEpisodesRequest>(r => r.Url == "https://api.thetvdb.com/series/4/episodes?page=1"),
                    Arg.Any<Option<string>>())
                .Returns(new Response<GetEpisodesRequest.Response>(new GetEpisodesRequest.Response(new[] { page1Episode },
                        new GetEpisodesRequest.PageLinks(1, 2, 2, Option<int>.None))));

            _tvDbConnection
                .GetAsync(Arg.Is<GetEpisodesRequest>(r => r.Url == "https://api.thetvdb.com/series/4/episodes?page=2"),
                    Arg.Any<Option<string>>())
                .Returns(new Response<GetEpisodesRequest.Response>(new GetEpisodesRequest.Response(new[] { page2Episode },
                        new GetEpisodesRequest.PageLinks(2, 2, Option<int>.None, 1))));

            var tvDbClient = new TvDbClient(_tvDbConnection, _fileCache, _applicationPaths, _logManager,
                new JsonSerialiser(), new PluginConfiguration());

            var episodes = await tvDbClient.GetEpisodesAsync(4);

            episodes.IsSome.Should().BeTrue();
            episodes.ValueUnsafe().Should().BeEquivalentTo(page1Episode, page2Episode);
        }

        [Test]
        public async Task GetEpisodesAsync_MultiPageResponse_SavesAllPagesConcatenated()
        {
            var page1Episode = new TvDbEpisodeData(1, "Test1", 1L, 2, 3, 4);
            var page2Episode = new TvDbEpisodeData(2, "Test2", 5L, 6, 7, 8);

            _tvDbConnection
                .GetAsync(Arg.Is<GetEpisodesRequest>(r => r.Url == "https://api.thetvdb.com/series/4/episodes?page=1"),
                    Arg.Any<Option<string>>())
                .Returns(new Response<GetEpisodesRequest.Response>(new GetEpisodesRequest.Response(new[] { page1Episode },
                        new GetEpisodesRequest.PageLinks(1, 2, 2, Option<int>.None))));

            _tvDbConnection
                .GetAsync(Arg.Is<GetEpisodesRequest>(r => r.Url == "https://api.thetvdb.com/series/4/episodes?page=2"),
                    Arg.Any<Option<string>>())
                .Returns(new Response<GetEpisodesRequest.Response>(new GetEpisodesRequest.Response(new[] { page2Episode },
                        new GetEpisodesRequest.PageLinks(2, 2, Option<int>.None, 1))));

            var tvDbClient = new TvDbClient(_tvDbConnection, _fileCache, _applicationPaths, _logManager,
                new JsonSerialiser(), new PluginConfiguration());

            await tvDbClient.GetEpisodesAsync(4);

            _fileCache.Received(1)
                .SaveFile(Arg.Any<TvDbSeriesEpisodesFileSpec>(),
                    Arg.Is<TvDbEpisodeCollection>(d => d.Episodes.SequenceEqual(new[] { page1Episode, page2Episode })));
        }

        [Test]
        public async Task GetSeriesAsync_FailedResponse_ReturnsNone()
        {
            _tvDbConnection.GetAsync(Arg.Any<GetSeriesRequest>(), Arg.Any<Option<string>>())
                .Returns(new FailedRequest(HttpStatusCode.BadRequest, "Failed"));

            var tvDbClient = new TvDbClient(_tvDbConnection, _fileCache, _applicationPaths, _logManager,
                new JsonSerialiser(), new PluginConfiguration());

            var seriesResult = await tvDbClient.GetSeriesAsync(4);

            seriesResult.IsSome.Should().BeFalse();
        }

        [Test]
        public async Task GetSeriesAsync_LocalSeriesData_DoesNotRequestSeriesData()
        {
            var series = new TvDbSeriesData(4, "TestSeries", new DateTime(2017, 1, 1, 1, 1, 1), "", 2,
                DayOfWeek.Monday, "", 4f, new[] { "Alias1", "Alias2" }, new[] { "Genre1", "Genre2" },
                "Overview");

            _fileCache.GetFileContent(Arg.Any<TvDbSeriesFileSpec>())
                .Returns(series);

            _tvDbConnection.GetAsync(Arg.Any<GetSeriesRequest>(), Arg.Any<Option<string>>())
                .Returns(new Response<GetSeriesRequest.Response>(new GetSeriesRequest.Response(series)));

            var tvDbClient = new TvDbClient(_tvDbConnection, _fileCache, _applicationPaths, _logManager,
                new JsonSerialiser(), new PluginConfiguration());

            await tvDbClient.GetSeriesAsync(4);

            _tvDbConnection.DidNotReceiveWithAnyArgs()
                .GetAsync<GetSeriesRequest.Response>(null, Option<string>.None);
        }

        [Test]
        public async Task GetSeriesAsync_LocalSeriesData_ReturnsLocalSeriesData()
        {
            var series = new TvDbSeriesData(4, "TestSeries", new DateTime(2017, 1, 1, 1, 1, 1), "", 2,
                DayOfWeek.Monday, "", 4f, new[] { "Alias1", "Alias2" }, new[] { "Genre1", "Genre2" },
                "Overview");

            _fileCache.GetFileContent(Arg.Any<TvDbSeriesFileSpec>())
                .Returns(series);

            _tvDbConnection.GetAsync(Arg.Any<GetSeriesRequest>(), Arg.Any<Option<string>>())
                .Returns(new Response<GetSeriesRequest.Response>(new GetSeriesRequest.Response(series)));

            var tvDbClient = new TvDbClient(_tvDbConnection, _fileCache, _applicationPaths, _logManager,
                new JsonSerialiser(), new PluginConfiguration());

            var seriesResult = await tvDbClient.GetSeriesAsync(4);

            seriesResult.IsSome.Should().BeTrue();
            seriesResult.ValueUnsafe().ShouldBeEquivalentTo(series);
        }


        [Test]
        public async Task GetSeriesAsync_NoLocalSeriesData_RequestsSeriesData()
        {
            var series = new TvDbSeriesData(4, "TestSeries", new DateTime(2017, 1, 1, 1, 1, 1), "", 2,
                DayOfWeek.Monday, "", 4f, new[] { "Alias1", "Alias2" }, new[] { "Genre1", "Genre2" },
                "Overview");

            _tvDbConnection.GetAsync(Arg.Any<GetSeriesRequest>(), Arg.Any<Option<string>>())
                .Returns(new Response<GetSeriesRequest.Response>(new GetSeriesRequest.Response(series)));

            var tvDbClient = new TvDbClient(_tvDbConnection, _fileCache, _applicationPaths, _logManager,
                new JsonSerialiser(), new PluginConfiguration());

            await tvDbClient.GetSeriesAsync(4);

            _tvDbConnection.ReceivedWithAnyArgs(1)
                .GetAsync<GetSeriesRequest.Response>(null, Option<string>.None);
        }

        [Test]
        public async Task GetSeriesAsync_NoLocalSeriesData_ReturnsNewSeriesData()
        {
            var series = new TvDbSeriesData(4, "TestSeries", new DateTime(2017, 1, 1, 1, 1, 1), "", 2,
                DayOfWeek.Monday, "", 4f, new[] { "Alias1", "Alias2" }, new[] { "Genre1", "Genre2" },
                "Overview");

            _tvDbConnection.GetAsync(Arg.Any<GetSeriesRequest>(), Arg.Any<Option<string>>())
                .Returns(new Response<GetSeriesRequest.Response>(new GetSeriesRequest.Response(series)));

            var tvDbClient = new TvDbClient(_tvDbConnection, _fileCache, _applicationPaths, _logManager,
                new JsonSerialiser(), new PluginConfiguration());

            var seriesResult = await tvDbClient.GetSeriesAsync(4);
            seriesResult.IsSome.Should().BeTrue();
            seriesResult.ValueUnsafe().Should().Be(series);
        }

        [Test]
        public async Task GetSeriesAsync_NoLocalSeriesData_SavesNewSeriesData()
        {
            var series = new TvDbSeriesData(4, "TestSeries", new DateTime(2017, 1, 1, 1, 1, 1), "", 2,
                DayOfWeek.Monday, "", 4f, new[] { "Alias1", "Alias2" }, new[] { "Genre1", "Genre2" },
                "Overview");

            _tvDbConnection.GetAsync(Arg.Any<GetSeriesRequest>(), Arg.Any<Option<string>>())
                .Returns(new Response<GetSeriesRequest.Response>(new GetSeriesRequest.Response(series)));

            var tvDbClient = new TvDbClient(_tvDbConnection, _fileCache, _applicationPaths, _logManager,
                new JsonSerialiser(), new PluginConfiguration());

            await tvDbClient.GetSeriesAsync(4);

            _fileCache.Received(1)
                .SaveFile(Arg.Any<TvDbSeriesFileSpec>(),
                    Arg.Is<TvDbSeriesData>(d => d == series));
        }
    }
}