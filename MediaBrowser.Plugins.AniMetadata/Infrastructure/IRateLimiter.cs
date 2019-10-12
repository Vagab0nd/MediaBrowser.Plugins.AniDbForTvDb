namespace MediaBrowser.Plugins.AniMetadata.Infrastructure
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IRateLimiter
    {
        SemaphoreSlim Semaphore { get; }
        Task TickAsync();
    }
}