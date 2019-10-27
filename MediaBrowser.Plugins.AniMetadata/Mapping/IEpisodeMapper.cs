using Emby.AniDbMetaStructure.AniDb.SeriesData;
using Emby.AniDbMetaStructure.TvDb.Data;
using LanguageExt;

namespace Emby.AniDbMetaStructure.Mapping
{
    internal interface IEpisodeMapper
    {
        OptionAsync<TvDbEpisodeData> MapAniDbEpisodeAsync(int aniDbEpisodeIndex,
            ISeriesMapping seriesMapping, Option<EpisodeGroupMapping> episodeGroupMapping);

        OptionAsync<AniDbEpisodeData> MapTvDbEpisodeAsync(int tvDbEpisodeIndex,
            ISeriesMapping seriesMapping, Option<EpisodeGroupMapping> episodeGroupMapping);
    }
}