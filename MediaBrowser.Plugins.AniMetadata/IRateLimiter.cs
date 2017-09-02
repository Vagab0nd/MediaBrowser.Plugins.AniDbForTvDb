using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.AniMetadata
{
    public interface IRateLimiter
    {
        SemaphoreSlim Semaphore { get; }
        Task TickAsync();
    }
}