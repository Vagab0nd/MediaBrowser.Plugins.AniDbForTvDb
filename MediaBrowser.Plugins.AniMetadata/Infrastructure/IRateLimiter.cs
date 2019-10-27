using System.Threading;
using System.Threading.Tasks;

namespace Emby.AniDbMetaStructure.Infrastructure
{
    public interface IRateLimiter
    {
        SemaphoreSlim Semaphore { get; }
        Task TickAsync();
    }
}