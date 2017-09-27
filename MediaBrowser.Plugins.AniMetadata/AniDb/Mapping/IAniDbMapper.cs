using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.Mapping
{
    public interface IAniDbMapper
    {
        Option<SeriesIds> GetMappedSeriesIdsFromAniDb(int aniDbSeriesId);

        Option<SeriesIds> GetMappedSeriesIdsFromTvDb(int tvDbSeriesId);

        Task<MappedEpisodeResult>
            GetMappedTvDbEpisodeIdAsync(int aniDbSeriesId, IAniDbEpisodeNumber aniDbEpisodeNumber);
    }
}