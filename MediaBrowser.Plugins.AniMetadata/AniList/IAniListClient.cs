using System.Collections.Generic;
using System.Threading.Tasks;
using Emby.AniDbMetaStructure.AniList.Data;
using Emby.AniDbMetaStructure.Process;
using LanguageExt;

namespace Emby.AniDbMetaStructure.AniList
{
    internal interface IAniListClient
    {
        Task<Either<ProcessFailedResult, IEnumerable<AniListSeriesData>>> FindSeriesAsync(string title,
            ProcessResultContext resultContext);
    }
}