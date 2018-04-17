using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniList;
using MediaBrowser.Plugins.AniMetadata.AniList.Requests;
using MediaBrowser.Plugins.AniMetadata.JsonApi;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
using NSubstitute;
using NUnit.Framework;
using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata.Tests.AniList
{
    [TestFixture]
    public class AniListTokenTests
    {
        [SetUp]
        public void Setup()
        {
            _resultContext = TestProcessResultContext.Instance;
            _jsonConnection = Substitute.For<IJsonConnection>();
            _aniListConfiguration = Substitute.For<IAnilistConfiguration>();

            _token = new AniListToken();
        }

        private IJsonConnection _jsonConnection;
        private IAnilistConfiguration _aniListConfiguration;
        private AniListToken _token;
        private ProcessResultContext _resultContext;

        [Test]
        public async Task GetToken_FailedRequest_ReturnsNone()
        {
            _jsonConnection.PostAsync<GetTokenRequest.TokenData>(null, null)
                .ReturnsForAnyArgs(new FailedRequest(HttpStatusCode.NotFound, "NotFound"));

            var result = await _token.GetToken(_jsonConnection, _aniListConfiguration, _resultContext);

            result.IsRight.Should().BeFalse();
        }

        [Test]
        public async Task GetToken_MultipleSimultaneousCalls_MakesOneRequest()
        {
            _jsonConnection.PostAsync<GetTokenRequest.TokenData>(null, null)
                .ReturnsForAnyArgs(x => Task.Delay(1000)
                    .ContinueWith(t => Right<FailedRequest, Response<GetTokenRequest.TokenData>>(
                        new Response<GetTokenRequest.TokenData>(
                            new GetTokenRequest.TokenData("AccessToken", 3, "RefreshToken")))));

            await Task.WhenAll(
                _token.GetToken(_jsonConnection, _aniListConfiguration, _resultContext),
                _token.GetToken(_jsonConnection, _aniListConfiguration, _resultContext),
                _token.GetToken(_jsonConnection, _aniListConfiguration, _resultContext),
                _token.GetToken(_jsonConnection, _aniListConfiguration, _resultContext),
                _token.GetToken(_jsonConnection, _aniListConfiguration, _resultContext),
                _token.GetToken(_jsonConnection, _aniListConfiguration, _resultContext));

            _jsonConnection.Received(1)
                .PostAsync(Arg.Any<GetTokenRequest>(), Arg.Is(Option<string>.None));
        }
        
        [Test]
        public async Task GetToken_PostsAuthorisationCode()
        {
            _jsonConnection.PostAsync<GetTokenRequest.TokenData>(null, null)
                .ReturnsForAnyArgs(new Response<GetTokenRequest.TokenData>(
                    new GetTokenRequest.TokenData("AccessToken", 3, "RefreshToken")));

            _aniListConfiguration.AuthorisationCode.Returns("AuthCode");

            await _token.GetToken(_jsonConnection, _aniListConfiguration, _resultContext);

            _jsonConnection.Received(1)
                .PostAsync(Arg.Any<GetTokenRequest>(), Arg.Is(Option<string>.None));

            var receivedCall = _jsonConnection.ReceivedCalls().Single();

            var receivedRequest = (GetTokenRequest)receivedCall.GetArguments()[0];
            receivedRequest.Url.Should().Be("https://anilist.co/api/v2/oauth/token");
            receivedRequest.Data.Should()
                .BeEquivalentTo(new
                {
                    grant_type = "authorization_code",
                    client_id = "362",
                    client_secret = "NSjmeTEekFlV9OZuZo9iR0BERNe3KS83iaIiI7EQ",
                    redirect_uri = "http://localhost:8096/web/configurationpage?name=AniMetadata",
                    code = "AuthCode"
                });
        }

        [Test]
        public async Task GetToken_SuccessfulRequest_ReturnsAccessToken()
        {
            _jsonConnection.PostAsync<GetTokenRequest.TokenData>(null, null)
                .ReturnsForAnyArgs(new Response<GetTokenRequest.TokenData>(
                    new GetTokenRequest.TokenData("AccessToken", 3, "RefreshToken")));

            var result = await _token.GetToken(_jsonConnection, _aniListConfiguration, _resultContext);

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Should().Be("AccessToken"));
        }
    }
}