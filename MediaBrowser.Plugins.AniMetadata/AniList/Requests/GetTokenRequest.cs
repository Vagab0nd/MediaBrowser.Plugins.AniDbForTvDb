using MediaBrowser.Plugins.AniMetadata.JsonApi;

namespace MediaBrowser.Plugins.AniMetadata.AniList.Requests
{
    internal class GetTokenRequest : Request<GetTokenRequest.TokenData>, IPostRequest<GetTokenRequest.TokenData>
    {
        public GetTokenRequest(int clientId, string clientSecret, string redirectUrl, string authorisationCode) : base(
            "https://anilist.co/api/v2/oauth/token")
        {
            Data = new
            {
                grant_type = "authorization_code",
                client_id = clientId.ToString(),
                client_secret = clientSecret,
                redirect_uri = redirectUrl,
                code = authorisationCode
            };
        }

        public object Data { get; }

        public class TokenData
        {
            public TokenData(string access_token, int expires_in, string refresh_token)
            {
                AccessToken = access_token;
                ExpiresIn = expires_in;
                RefreshToken = refresh_token;
            }

            public string AccessToken { get; }

            public int ExpiresIn { get; }

            public string RefreshToken { get; }
        }
    }
}