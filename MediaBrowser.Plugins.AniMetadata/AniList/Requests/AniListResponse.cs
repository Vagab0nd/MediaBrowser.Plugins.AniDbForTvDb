using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.AniList.Requests
{
    internal class AniListResponse<TResponseData>
    {
        public AniListResponse(TResponseData data, Option<int> rateLimit, Option<int> rateLimitRemaining)
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