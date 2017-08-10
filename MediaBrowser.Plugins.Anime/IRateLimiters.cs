namespace MediaBrowser.Plugins.Anime
{
    public interface IRateLimiters
    {
        IRateLimiter AniDb { get; }
    }
}