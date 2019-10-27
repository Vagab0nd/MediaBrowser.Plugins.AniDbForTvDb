using Emby.AniDbMetaStructure.AniDb.SeriesData;
using Emby.AniDbMetaStructure.TvDb.Data;
using LanguageExt;

namespace Emby.AniDbMetaStructure.Mapping
{
    internal interface IGroupMappingEpisodeMapper
    {
        OptionAsync<TvDbEpisodeData> MapAniDbEpisodeAsync(int aniDbEpisodeIndex,
            EpisodeGroupMapping episodeGroupMapping, int tvDbSeriesId);

        OptionAsync<AniDbEpisodeData> MapTvDbEpisodeAsync(int tvDbEpisodeIndex,
            EpisodeGroupMapping episodeGroupMapping, int aniDbSeriesId);
    }
}