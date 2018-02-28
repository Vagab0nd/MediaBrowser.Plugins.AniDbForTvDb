using System.Collections.Generic;
using System.Threading.Tasks;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Providers;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.Mapping
{
    public interface IDataMapper
    {
        Task<SeriesData> MapSeriesDataAsync(AniDbSeriesData aniDbSeriesData);

        Task<EpisodeData>
            MapEpisodeDataAsync(AniDbSeriesData aniDbSeriesData, AniDbEpisodeData aniDbEpisodeData);

        Task<IEnumerable<SeriesData>> MapSeriesDataAsync(TvDbSeriesData tvDbSeriesData);

        Task<EpisodeData> MapEpisodeDataAsync(int aniDbSeriesId, TvDbSeriesData tvDbSeriesData, TvDbEpisodeData tvDbEpisodeData);
    }
}