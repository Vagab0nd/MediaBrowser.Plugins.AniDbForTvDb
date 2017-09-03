using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata
{
    internal static class MaybeExtensions
    {
        public static TaskAwaiter<Option<T>> GetAwaiter<T>(this Option<Task<T>> taskOption)
        {
            return taskOption.Match(
                to => to.ContinueWith(t => (Option<T>)t.Result).GetAwaiter(),
                () => Task.FromResult(Option<T>.None).GetAwaiter());
        }
    }
}