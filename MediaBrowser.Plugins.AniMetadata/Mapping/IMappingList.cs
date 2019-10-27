using System.Collections.Generic;
using System.Threading.Tasks;
using Emby.AniDbMetaStructure.Process;
using LanguageExt;

namespace Emby.AniDbMetaStructure.Mapping
{
    internal interface IMappingList
    {
        Task<Either<ProcessFailedResult, ISeriesMapping>> GetSeriesMappingFromAniDb(int aniDbSeriesId,
            ProcessResultContext resultContext);

        Task<Either<ProcessFailedResult, IEnumerable<ISeriesMapping>>> GetSeriesMappingsFromTvDb(int tvDbSeriesId,
            ProcessResultContext resultContext);
    }
}