using System.Threading;
using System.Threading.Tasks;

namespace Emby.AniDbMetaStructure.Files
{
    internal interface ICustomDownload<in T> where T : class
    {
        Task<string> DownloadFileAsync(IRemoteFileSpec<T> fileSpec, CancellationToken cancellationToken);
    }
}
