namespace MediaBrowser.Plugins.AniMetadata
{
    public interface IRateLimiters
    {
        IRateLimiter AniDb { get; }
    }
}