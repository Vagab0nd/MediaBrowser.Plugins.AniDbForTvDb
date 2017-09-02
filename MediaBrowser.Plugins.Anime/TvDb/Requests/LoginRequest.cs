namespace MediaBrowser.Plugins.AniMetadata.TvDb.Requests
{
    internal class LoginRequest : PostRequest<LoginRequest.Response>
    {
        public LoginRequest(string apiKey) : base("login", new RequestData(apiKey))
        {
        }

        public class RequestData
        {
            public RequestData(string apiKey)
            {
                ApiKey = apiKey;
            }

            public string ApiKey { get; }
        }

        public class Response
        {
            public Response(string token)
            {
                Token = token;
            }

            public string Token { get; }
        }
    }
}