using MediaBrowser.Plugins.AniMetadata.JsonApi;

namespace MediaBrowser.Plugins.AniMetadata.AniList.Requests
{
    internal abstract class AniListQueryRequest<TResponse> : Request<TResponse>
    {
        private const string AniListUrl = "https://graphql.anilist.co";

        protected AniListQueryRequest(string query, string variables) : base(AniListUrl)
        {
            Query = query;
            Variables = variables;
        }

        public string Query { get; }

        public string Variables { get; }
    }
}