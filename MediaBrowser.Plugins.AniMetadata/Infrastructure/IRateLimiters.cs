namespace MediaBrowser.Plugins.AniMetadata.Infrastructure
{
    public interface IRateLimiters
    {
        IRateLimiter AniDb { get; }
    }
}