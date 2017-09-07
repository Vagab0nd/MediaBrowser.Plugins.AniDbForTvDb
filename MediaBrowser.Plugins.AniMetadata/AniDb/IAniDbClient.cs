using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping;
using MediaBrowser.Plugins.AniMetadata.AniDb.Seiyuu;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    public interface IAniDbClient
    {
        Task<Option<AniDbSeriesData>> FindSeriesAsync(string title);

        Task<Option<IAniDbMapper>> GetMapperAsync();

        Task<Option<AniDbSeriesData>> GetSeriesAsync(string aniDbSeriesIdString);

        IEnumerable<SeiyuuData> FindSeiyuu(string name);

        Option<SeiyuuData> GetSeiyuu(int seiyuuId);
    }
}