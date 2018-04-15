using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.Process;

namespace MediaBrowser.Plugins.AniMetadata.AniList
{
    internal interface IAniListToken
    {
        Task<Either<ProcessFailedResult, string>> GetToken(ProcessResultContext resultContext);
    }
}