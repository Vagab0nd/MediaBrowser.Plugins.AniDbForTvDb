using System.Threading.Tasks;
using FluentAssertions;
using MediaBrowser.Plugins.AniMetadata.AniList;
using MediaBrowser.Plugins.AniMetadata.JsonApi;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests.IntegrationTests.AniList
{
    [TestFixture]
    public class AniListTokenIntegrationTests
    {
        private IAnilistConfiguration _aniListConfiguration;
        private AniListToken _token;

        [SetUp]
        public void Setup()
        {
            var jsonConnection =
                new JsonConnection(new TestHttpClient(), new JsonSerialiser(), new ConsoleLogManager());
            _aniListConfiguration = Substitute.For<IAnilistConfiguration>();

            _token = new AniListToken(jsonConnection, _aniListConfiguration);
        }

        [Test]
        public async Task GetToken_InvalidConfiguration_ReturnsNone()
        {
            _aniListConfiguration.AuthorisationCode.Returns("InvalidCode");

            var result = await _token.GetToken();

            result.IsSome.Should().BeFalse();
        }

        [Test]
        public async Task GetToken_ValidConfiguration_ReturnsAccessToken()
        {
            _aniListConfiguration.AuthorisationCode.Returns("def502009de9f1bc407ff3e747febd0ffde0867482aedfc7b0adf37fb3ce6450f21a76e6eaa7d482f6b6e1ab017133c28d07608ae57a74ed3a27d27fe02fc8db1cff12ecb482beac3c91e14acc30de168299aff7f93efb1236612b8562b86afb19f1819b395ede8bfb4983df50199f191b812e4d7d3a80abc425d733bb78aa58f565673f95cf9984507c14d10dbb05fa3d67aa85325dce7c0209da022ec32e142b131ce5ca018ce8c1a103add47e3630fd066ef1896c94945cbb22c2e0d72b2c5f77a4b84afee730d8753f8c853023a7063ef87b2ff4c36c03d4b1231fedf88ffbdfa3b9e8128842264f939c29aff16fba9484699a458ef865f2ee0857d5747983472563101f070629558615c257efc51427e228ecfc3958a999e8c86da91d40c79da3cf247967c5e6e2abef1572bdeb55aa6222139776d6d80cc7a07a9aab2964aff66623c4f0d1cdf26baeb8153fad0188f7bdebddc772bc05e1e02fe2e5b0329d80108d61cf30f7b10d40f50f24db2bb6e637c1480929cc8890c7c2f509ee99594cbf262a");

            var result = await _token.GetToken();

            result.IsSome.Should().BeTrue();
            result.IfSome(r => r.Length.Should().BeGreaterThan(0));
        }
    }
}