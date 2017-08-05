using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    internal interface IFileDownloader
    {
        Task DownloadFileAsync(AniDbFileSpec fileSpec, CancellationToken cancellationToken);
    }
}