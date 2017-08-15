using System.Threading;
using System.Threading.Tasks;
using Functional.Maybe;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    internal interface IAniDbFileCache
    {
        Maybe<T> GetFileContent<T>(ILocalFileSpec<T> fileSpec) where T : class;

        Task<Maybe<T>> GetFileContentAsync<T>(IRemoteFileSpec<T> fileSpec, CancellationToken cancellationToken)
            where T : class;

        void SaveFile<T>(ILocalFileSpec<T> fileSpec, T data) where T : class;
    }
}