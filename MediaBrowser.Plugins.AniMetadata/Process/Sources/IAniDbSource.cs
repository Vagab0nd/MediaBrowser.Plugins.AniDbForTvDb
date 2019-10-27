using System.Collections.Generic;
using System.Threading.Tasks;
using Emby.AniDbMetaStructure.AniDb.SeriesData;
using LanguageExt;

namespace Emby.AniDbMetaStructure.Process.Sources
{
    internal interface IAniDbSource : ISource
    {
        Task<Either<ProcessFailedResult, AniDbSeriesData>> GetSeriesData(IEmbyItemData embyItemData,
            ProcessResultContext resultContext);

        Task<Either<ProcessFailedResult, AniDbSeriesData>> GetSeriesData(int aniDbSeriesId, ProcessResultContext resultContext);

        Either<ProcessFailedResult, string> SelectTitle(IEnumerable<ItemTitleData> titles,
            string metadataLanguage, ProcessResultContext resultContext);
    }
}