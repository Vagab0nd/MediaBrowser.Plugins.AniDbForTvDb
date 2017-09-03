using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using MediaBrowser.Plugins.AniMetadata.TvDb.Requests;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    internal class TvDbTokenTests
    {
        [SetUp]
        public void Setup()
        {
            _logManager = new ConsoleLogManager();
        }

        private ConsoleLogManager _logManager;

        [Test]
        public async Task GetToken_ExistingToken_DoesNotRequestNewToken()
        {
            var tvDbConnection = Substitute.For<ITvDbConnection>();
            tvDbConnection.PostAsync(Arg.Is<LoginRequest>(r =>
                    r.Url == "https://api.thetvdb.com/login" &&
                    (r.Data as LoginRequest.RequestData).ApiKey == "apiKey"), Option<string>.None)
                .Returns(new Response<LoginRequest.Response>(new LoginRequest.Response("TOKEN")));

            var token = new TvDbToken(tvDbConnection, "apiKey", _logManager);

            await token.GetTokenAsync();

            var returnedToken = await token.GetTokenAsync();

            returnedToken.IsSome.Should().BeTrue();
            returnedToken.ValueUnsafe().Should().Be("TOKEN");

            tvDbConnection.ReceivedWithAnyArgs(1).PostAsync<LoginRequest.Response>(null, Option<string>.None);
        }

        [Test]
        public async Task GetToken_FailedRequest_ReturnsNone()
        {
            var tvDbConnection = Substitute.For<ITvDbConnection>();
            tvDbConnection.PostAsync(Arg.Is<LoginRequest>(r =>
                    r.Url == "https://api.thetvdb.com/login" &&
                    (r.Data as LoginRequest.RequestData).ApiKey == "apiKey"), Option<string>.None)
                .Returns(new FailedRequest(HttpStatusCode.BadRequest, "Failed"));

            var token = new TvDbToken(tvDbConnection, "apiKey", _logManager);

            var returnedToken = await token.GetTokenAsync();

            returnedToken.IsSome.Should().BeFalse();
        }

        [Test]
        public async Task GetToken_NoExistingToken_RequestsToken()
        {
            var tvDbConnection = Substitute.For<ITvDbConnection>();
            tvDbConnection.PostAsync(Arg.Is<LoginRequest>(r =>
                    r.Url == "https://api.thetvdb.com/login" &&
                    (r.Data as LoginRequest.RequestData).ApiKey == "apiKey"), Option<string>.None)
                .Returns(new Response<LoginRequest.Response>(new LoginRequest.Response("TOKEN")));

            var token = new TvDbToken(tvDbConnection, "apiKey", _logManager);

            var returnedToken = await token.GetTokenAsync();

            returnedToken.IsSome.Should().BeTrue();
            returnedToken.ValueUnsafe().Should().Be("TOKEN");
        }
    }
}