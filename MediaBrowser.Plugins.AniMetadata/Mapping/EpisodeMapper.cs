using System.Threading.Tasks;
using LanguageExt;
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

        public Task<Option<TvDbEpisodeData>> MapEpisodeAsync(int aniDbEpisodeIndex,
            ISeriesMapping seriesMapping, Option<EpisodeGroupMapping> episodeGroupMapping)
        {
            return episodeGroupMapping.Match(
                m => seriesMapping.Ids.TvDbSeriesId.MatchAsync(
                    tvDbSeriesId =>
                        _groupMappingEpisodeMapper.MapEpisodeAsync(aniDbEpisodeIndex, m,
                            tvDbSeriesId),
                    () => Option<TvDbEpisodeData>.None),
                () => _defaultSeasonEpisodeMapper.MapEpisodeAsync(aniDbEpisodeIndex,
                    seriesMapping));
        }
    }
}