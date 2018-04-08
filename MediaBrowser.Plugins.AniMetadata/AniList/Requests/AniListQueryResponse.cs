using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.AniList.Requests
{
    internal class AniListQueryResponse<TResponseData>
    {
        public AniListQueryResponse(TResponseData data, Option<int> rateLimit, Option<int> rateLimitRemaining)
        {
            Data = data;
            RateLimit = rateLimit;
            RateLimitRemaining = rateLimitRemaining;
        }

        public TResponseData Data { get; }

        public Option<int> RateLimit { get; }

        public Option<int> RateLimitRemaining { get; }
    }
}