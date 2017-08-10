using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.Anime
{
    public interface IRateLimiter
    {
        Task TickAsync();

        SemaphoreSlim Semaphore { get; }
    }
}