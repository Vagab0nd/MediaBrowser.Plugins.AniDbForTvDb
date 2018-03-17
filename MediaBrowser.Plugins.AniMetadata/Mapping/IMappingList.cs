using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.Process;

namespace MediaBrowser.Plugins.AniMetadata.Mapping
{
    internal interface IMappingList
    {
        Task<Either<ProcessFailedResult, ISeriesMapping>> GetSeriesMappingFromAniDb(int aniDbSeriesId,
            ProcessResultContext resultContext);

        Task<Either<ProcessFailedResult, IEnumerable<ISeriesMapping>>> GetSeriesMappingsFromTvDb(int tvDbSeriesId,
            ProcessResultContext resultContext);
    }
}