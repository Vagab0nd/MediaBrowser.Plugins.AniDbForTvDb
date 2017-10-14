using System.Threading.Tasks;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Providers;

namespace MediaBrowser.Plugins.AniMetadata.Mapping
{
    public interface IDataMapper
    {
        Task<SeriesData> MapSeriesDataAsync(AniDbSeriesData aniDbSeriesData);

        Task<EpisodeData>
            MapEpisodeDataAsync(AniDbSeriesData aniDbSeriesData, AniDbEpisodeData aniDbEpisodeData);
    }
}