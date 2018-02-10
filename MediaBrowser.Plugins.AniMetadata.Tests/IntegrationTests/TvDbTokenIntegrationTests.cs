using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt.UnsafeValueAccess;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests.IntegrationTests
{
    [TestFixture]
    [Explicit]
    internal class TvDbTokenIntegrationTests
    {
        [SetUp]
        public void Setup()
        {
            _logManager = new ConsoleLogManager();
        }

        private ILogManager _logManager;

        [Test]
        public async Task GetToken_ExistingToken_DoesNotRequestNewToken()
        {
            var tvDbConnection = new TvDbConnection(new TestHttpClient(), new JsonSerialiser(), _logManager);

            var token = new TvDbToken(tvDbConnection, Secrets.TvDbApiKey, _logManager);

            var token1 = await token.GetTokenAsync();

            var token2 = await token.GetTokenAsync();

            token2.IsSome.Should().BeTrue();
            token2.ValueUnsafe().Should().Be(token1.ValueUnsafe());
        }

        [Test]
        public async Task GetToken_FailedRequest_ReturnsNone()
        {
            var tvDbConnection = new TvDbConnection(new TestHttpClient(), new JsonSerialiser(), _logManager);

            var token = new TvDbToken(tvDbConnection, "NotValid", _logManager);

            var returnedToken = await token.GetTokenAsync();

            returnedToken.IsSome.Should().BeFalse();
        }

        [Test]
        public async Task GetToken_NoExistingToken_GetsNewToken()
        {
            var tvDbConnection = new TvDbConnection(new TestHttpClient(), new JsonSerialiser(), _logManager);

            var token = new TvDbToken(tvDbConnection, Secrets.TvDbApiKey, _logManager);

            var returnedToken = await token.GetTokenAsync();

            returnedToken.IsSome.Should().BeTrue();
            returnedToken.ValueUnsafe().Should().NotBeNullOrWhiteSpace();
        }
    }
}