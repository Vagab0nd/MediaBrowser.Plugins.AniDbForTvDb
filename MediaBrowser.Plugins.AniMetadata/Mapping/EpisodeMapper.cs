using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.Mapping
{
    internal class EpisodeMapper : IEpisodeMapper
    {
        private readonly IDefaultSeasonEpisodeMapper _defaultSeasonEpisodeMapper;
        private readonly IGroupMappingEpisodeMapper _groupMappingEpisodeMapper;

        public EpisodeMapper(IDefaultSeasonEpisodeMapper defaultSeasonEpisodeMapper,
            IGroupMappingEpisodeMapper groupMappingEpisodeMapper)
        {
            _defaultSeasonEpisodeMapper = defaultSeasonEpisodeMapper;
            _groupMappingEpisodeMapper = groupMappingEpisodeMapper;
        }

        public OptionAsync<TvDbEpisodeData> MapAniDbEpisodeAsync(int aniDbEpisodeIndex,
            ISeriesMapping seriesMapping, Option<EpisodeGroupMapping> episodeGroupMapping)
        {
            return episodeGroupMapping.Match(
                m => seriesMapping.Ids.TvDbSeriesId.BindAsync(
                    tvDbSeriesId =>
                        _groupMappingEpisodeMapper.MapAniDbEpisodeAsync(aniDbEpisodeIndex, m,
                            tvDbSeriesId)),
                () => _defaultSeasonEpisodeMapper.MapEpisodeAsync(aniDbEpisodeIndex,
                        seriesMapping)
                    .ToAsync());
        }

        public OptionAsync<AniDbEpisodeData> MapTvDbEpisodeAsync(int tvDbEpisodeIndex,
            ISeriesMapping seriesMapping, Option<EpisodeGroupMapping> episodeGroupMapping)
        {
            return episodeGroupMapping.BindAsync(
                m => _groupMappingEpisodeMapper.MapTvDbEpisodeAsync(tvDbEpisodeIndex, m,
                    seriesMapping.Ids.AniDbSeriesId));
        }
    }
}