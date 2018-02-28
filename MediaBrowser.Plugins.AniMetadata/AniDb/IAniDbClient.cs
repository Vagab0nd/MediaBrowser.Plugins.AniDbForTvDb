using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb.Seiyuu;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
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