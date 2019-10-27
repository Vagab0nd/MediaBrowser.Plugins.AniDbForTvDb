namespace Emby.AniDbMetaStructure.Infrastructure
{
    public interface IRateLimiters
    {
        IRateLimiter AniDb { get; }
    }
}