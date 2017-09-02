using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.AniMetadata.Files
{
    internal interface IFileDownloader
    {
        Task DownloadFileAsync<T>(IRemoteFileSpec<T> fileSpec, CancellationToken cancellationToken) where T : class;
    }
}