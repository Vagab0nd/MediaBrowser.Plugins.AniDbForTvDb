using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb.Series.Data;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.Mapping
{
    public interface IAniDbMapper
    {
        Option<SeriesIds> GetMappedSeriesIds(int aniDbSeriesId);

        Task<MappedEpisodeResult>
            GetMappedTvDbEpisodeIdAsync(int aniDbSeriesId, IAniDbEpisodeNumber aniDbEpisodeNumber);
    }
}