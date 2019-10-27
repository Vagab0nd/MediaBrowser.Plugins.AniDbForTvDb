using Emby.AniDbMetaStructure.JsonApi;

namespace Emby.AniDbMetaStructure.TvDb.Requests
{
    internal class LoginRequest : TvDbRequest<GetEpisodesRequest.Response>, IPostRequest<LoginRequest.Response>
    {
        public LoginRequest(string apiKey) : base("login")
        {
            this.Data = new RequestData(apiKey);
        }

        public object Data { get; }

        public class RequestData
        {
            public RequestData(string apiKey)
            {
                this.ApiKey = apiKey;
            }

            public string ApiKey { get; }
        }

        public class Response
        {
            public Response(string token)
            {
                this.Token = token;
            }

            public string Token { get; }
        }
    }
}