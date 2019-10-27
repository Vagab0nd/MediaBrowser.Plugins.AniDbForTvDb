using System.Collections.Generic;
using System.Threading.Tasks;
using Emby.AniDbMetaStructure.AniDb.Seiyuu;
using Emby.AniDbMetaStructure.AniDb.SeriesData;
using LanguageExt;

namespace Emby.AniDbMetaStructure.AniDb
{
    public interface IAniDbClient
    {
        Task<Option<AniDbSeriesData>> FindSeriesAsync(string title);

        Task<Option<AniDbSeriesData>> GetSeriesAsync(string aniDbSeriesIdString);

        Task<Option<AniDbSeriesData>> GetSeriesAsync(int aniDbSeriesId);

        IEnumerable<SeiyuuData> FindSeiyuu(string name);

        Option<SeiyuuData> GetSeiyuu(int seiyuuId);
    }
}