using System.Threading.Tasks;
using Emby.AniDbMetaStructure.TvDb.Data;
using LanguageExt;

namespace Emby.AniDbMetaStructure.Process.Sources
{
    internal interface ITvDbSource : ISource
    {
        Task<Either<ProcessFailedResult, TvDbSeriesData>> GetSeriesData(IEmbyItemData embyItemData,
            ProcessResultContext resultContext);

        Task<Either<ProcessFailedResult, TvDbSeriesData>> GetSeriesData(int tvDbSeriesId,
            ProcessResultContext resultContext);
    }
}