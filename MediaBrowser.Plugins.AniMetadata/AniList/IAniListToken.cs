using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.JsonApi;
using MediaBrowser.Plugins.AniMetadata.Process;

namespace MediaBrowser.Plugins.AniMetadata.AniList
{
    internal interface IAniListToken
    {
        Task<Either<FailedRequest, string>> GetToken(IJsonConnection jsonConnection,
            IAnilistConfiguration anilistConfiguration, ProcessResultContext resultContext);
    }
}