using System.Collections.Generic;
using System.Threading.Tasks;
using Functional.Maybe;
using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping;
using MediaBrowser.Plugins.AniMetadata.AniDb.Seiyuu;
using MediaBrowser.Plugins.AniMetadata.AniDb.Series.Data;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    public interface IAniDbClient
    {
        Task<Maybe<AniDbSeriesData>> FindSeriesAsync(string title);

        Task<Maybe<IAniDbMapper>> GetMapperAsync();

        Task<Maybe<AniDbSeriesData>> GetSeriesAsync(string aniDbSeriesIdString);

        IEnumerable<SeiyuuData> FindSeiyuu(string name);

        Maybe<SeiyuuData> GetSeiyuu(int seiyuuId);
    }
}