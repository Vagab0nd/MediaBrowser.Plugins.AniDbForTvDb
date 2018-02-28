using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.Mapping
{
    internal interface IEpisodeMapper
    {
        OptionAsync<TvDbEpisodeData> MapAniDbEpisodeAsync(int aniDbEpisodeIndex,
            ISeriesMapping seriesMapping, Option<EpisodeGroupMapping> episodeGroupMapping);

        OptionAsync<AniDbEpisodeData> MapTvDbEpisodeAsync(int tvDbEpisodeIndex,
            ISeriesMapping seriesMapping, Option<EpisodeGroupMapping> episodeGroupMapping);
    }
}