using System.Threading.Tasks;
using Emby.AniDbMetaStructure.JsonApi;
using Emby.AniDbMetaStructure.Process;
using LanguageExt;

namespace Emby.AniDbMetaStructure.AniList
{
    internal interface IAniListToken
    {
        Task<Either<FailedRequest, string>> GetToken(IJsonConnection jsonConnection,
            IAnilistConfiguration anilistConfiguration, ProcessResultContext resultContext);
    }
}