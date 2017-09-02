using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Functional.Maybe;

namespace MediaBrowser.Plugins.AniMetadata
{
    internal static class MaybeExtensions
    {
        public static TaskAwaiter<Maybe<T>> GetAwaiter<T>(this Maybe<Task<T>> taskMaybe)
        {
            if (taskMaybe.HasValue)
            {
                return taskMaybe.Value.ContinueWith(t => t.Result.ToMaybe()).GetAwaiter();
            }

            return Task.FromResult(Maybe<T>.Nothing).GetAwaiter();
        }
    }
}