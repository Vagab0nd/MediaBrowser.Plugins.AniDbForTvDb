using System.Threading;
using System.Threading.Tasks;

namespace Emby.AniDbMetaStructure.Files
{
    internal interface IFileDownloader
    {
        Task DownloadFileAsync<T>(IRemoteFileSpec<T> fileSpec, CancellationToken cancellationToken) where T : class;
    }
}