using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniList;
using MediaBrowser.Plugins.AniMetadata.AniList.Requests;
using MediaBrowser.Plugins.AniMetadata.JsonApi;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests.AniList
{
    [TestFixture]
    public class AniListTokenTests
    {
        [SetUp]
        public void Setup()
        {
            _jsonConnection = Substitute.For<IJsonConnection>();
            _aniListConfiguration = Substitute.For<IAnilistConfiguration>();

            _token = new AniListToken(_jsonConnection, _aniListConfiguration);
        }

        private IJsonConnection _jsonConnection;
        private IAnilistConfiguration _aniListConfiguration;
        private AniListToken _token;

        [Test]
        public async Task GetToken_FailedRequest_ReturnsNone()
        {
            _jsonConnection.PostAsync<GetTokenRequest.TokenData>(null, null)
                .ReturnsForAnyArgs(new FailedRequest(HttpStatusCode.NotFound, "NotFound"));

            var result = await _token.GetToken();

            result.IsSome.Should().BeFalse();
        }

        [Test]
        public async Task GetToken_PostsAuthorisationCode()
        {
            _jsonConnection.PostAsync<GetTokenRequest.TokenData>(null, null)
                .ReturnsForAnyArgs(new Response<GetTokenRequest.TokenData>(
                    new GetTokenRequest.TokenData("AccessToken", 3, "RefreshToken")));

            _aniListConfiguration.AuthorisationCode.Returns("AuthCode");

            await _token.GetToken();

            _jsonConnection.Received(1)
                .PostAsync(Arg.Any<GetTokenRequest>(), Arg.Is(Option<string>.None));

            var receivedCall = _jsonConnection.ReceivedCalls().Single();

            var receivedRequest = (GetTokenRequest)receivedCall.GetArguments()[0];
            receivedRequest.Url.Should().Be( "https://anilist.co/api/v2/oauth/token");
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

            var result = await _token.GetToken();

            result.IsSome.Should().BeTrue();
            result.IfSome(r => r.Should().Be("AccessToken"));
        }
    }
}