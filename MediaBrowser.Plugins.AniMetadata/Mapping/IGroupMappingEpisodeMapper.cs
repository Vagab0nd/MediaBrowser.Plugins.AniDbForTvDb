using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.Mapping
{
    internal interface IGroupMappingEpisodeMapper
    {
        OptionAsync<TvDbEpisodeData> MapAniDbEpisodeAsync(int aniDbEpisodeIndex,
            EpisodeGroupMapping episodeGroupMapping, int tvDbSeriesId);

        OptionAsync<AniDbEpisodeData> MapTvDbEpisodeAsync(int tvDbEpisodeIndex,
            EpisodeGroupMapping episodeGroupMapping, int aniDbSeriesId);
    }
}