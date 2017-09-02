using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.Anime
{
    public interface IRateLimiter
    {
        SemaphoreSlim Semaphore { get; }
        Task TickAsync();
    }
}