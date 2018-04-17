using System.Threading.Tasks;
using FluentAssertions;
using MediaBrowser.Plugins.AniMetadata.AniList;
using MediaBrowser.Plugins.AniMetadata.JsonApi;
using MediaBrowser.Plugins.AniMetadata.Process;
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
        private ProcessResultContext _resultContext;
        private JsonConnection _jsonConnection;

        [SetUp]
        public void Setup()
        {
            _resultContext = TestProcessResultContext.Instance;

            _jsonConnection =
                new JsonConnection(new TestHttpClient(), new JsonSerialiser(), new ConsoleLogManager());
            _aniListConfiguration = Substitute.For<IAnilistConfiguration>();

            _token = new AniListToken();
        }

        [Test]
        public async Task GetToken_InvalidConfiguration_ReturnsNone()
        {
            _aniListConfiguration.AuthorisationCode.Returns("InvalidCode");

            var result = await _token.GetToken(_jsonConnection,_aniListConfiguration, _resultContext);

            result.IsRight.Should().BeFalse();
        }

        [Test]
        [Explicit("New authorisation code must be generated once it expires")]
        public async Task GetToken_ValidConfiguration_ReturnsAccessToken()
        {
            _aniListConfiguration.AuthorisationCode.Returns("def50200839d9ea8517403a507d4b8ff69c4de84ec0881987ffcd0cd2302ee827be75224d0e0ec28816157ef202a3e788c4c2e8e41e94d4c37c42aa2e5cdd2f805e621a3ac37a74c075bdd3645209f5daf8394310bfcee40be9d68faf58efef3abcfadfcfdf08b7e6aadfae47662dfc66e701f57b45fb664508f8018ebf94ba0d096f6e66601c9e5b8a3d2114f1bf9a4865420d3eb9fd2e4a22c37be48c01b9f51a6de2fefb88268bf536370aed23a1f9d02f2c06c9b01d35cb892884cd519e70ccf2b3a44a0b299d9789f8da474e5a8c618762eb0d071cca1c222461cb685d428fa2dcb98e2c7db061d2c45a0d70b9569a9fbf7ee546cbf8f8001574db74604c1cc5613ec71f11c509dcaeb0cc6ce441947e873edef86612b19a958e43aaa38885748c8b1e69909c2748853292837251cc01a11db013b851081f4f17f0a69ddfdb6f88c823a9d5f0984d1c12f724541711efbc7f4604e944ffd7002334f61888c50ce97e7aa0bc6d24a4e0eefb4afc2f772655c3653bae841449a2ebd72ef39f0b20b4463fe");

            var result = await _token.GetToken(_jsonConnection, _aniListConfiguration,_resultContext);

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Length.Should().BeGreaterThan(0));
        }
    }
}