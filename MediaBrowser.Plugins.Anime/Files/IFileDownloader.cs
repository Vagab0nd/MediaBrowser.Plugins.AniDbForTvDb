using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.Anime.Files
{
    internal interface IFileDownloader
    {
        Task DownloadFileAsync<T>(IRemoteFileSpec<T> fileSpec, CancellationToken cancellationToken) where T : class;
    }
}