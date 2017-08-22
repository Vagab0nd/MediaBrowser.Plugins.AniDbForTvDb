using Functional.Maybe;
using MediaBrowser.Plugins.Anime.AniDb.Series.Data;

namespace MediaBrowser.Plugins.Anime.AniDb.Mapping
{
    public interface IAniDbMapper
    {
        Maybe<SeriesIds> GetMappedSeriesIds(int aniDbSeriesId);

        MappedEpisodeResult GetMappedTvDbEpisodeId(int aniDbSeriesId, IAniDbEpisodeNumber aniDbEpisodeNumber);
    }
}