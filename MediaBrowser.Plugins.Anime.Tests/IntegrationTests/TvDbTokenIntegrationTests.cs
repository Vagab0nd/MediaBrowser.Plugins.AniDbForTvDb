using System.Threading.Tasks;
using FluentAssertions;
using MediaBrowser.Plugins.Anime.Tests.TestHelpers;
using MediaBrowser.Plugins.Anime.TvDb;
using NUnit.Framework;

namespace MediaBrowser.Plugins.Anime.Tests.IntegrationTests
{
    [TestFixture]
    [Explicit]
    internal class TvDbTokenIntegrationTests
    {
        [Test]
        public async Task GetToken_ExistingToken_DoesNotRequestNewToken()
        {
            var tvDbConnection = new TvDbConnection(new TestHttpClient(), new TestJsonSerialiser());

            var token = new TvDbToken(tvDbConnection, Secrets.Instance.TvDbApiKey);

            var token1 = await token.GetTokenAsync();

            var token2 = await token.GetTokenAsync();

            token2.HasValue.Should().BeTrue();
            token2.Value.Should().Be(token1.Value);
        }

        [Test]
        public async Task GetToken_FailedRequest_ReturnsNone()
        {
            var tvDbConnection = new TvDbConnection(new TestHttpClient(), new TestJsonSerialiser());

            var token = new TvDbToken(tvDbConnection, "NotValid");

            var returnedToken = await token.GetTokenAsync();

            returnedToken.HasValue.Should().BeFalse();
        }

        [Test]
        public async Task GetToken_NoExistingToken_GetsNewToken()
        {
            var tvDbConnection = new TvDbConnection(new TestHttpClient(), new TestJsonSerialiser());

            var token = new TvDbToken(tvDbConnection, Secrets.Instance.TvDbApiKey);

            var returnedToken = await token.GetTokenAsync();

            returnedToken.HasValue.Should().BeTrue();
            returnedToken.Value.Should().NotBeNullOrWhiteSpace();
        }
    }
}