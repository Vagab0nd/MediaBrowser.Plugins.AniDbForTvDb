using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.Mapping
{
    /// <summary>
    ///     Maps an AniDb episode to a TvDb episode using an <see cref="EpisodeGroupMapping" />
    /// </summary>
    internal class GroupMappingEpisodeMapper : IGroupMappingEpisodeMapper
    {
        private readonly ILogger _log;
        private readonly ITvDbClient _tvDbClient;

        public GroupMappingEpisodeMapper(ITvDbClient tvDbClient, ILogManager logManager)
        {
            _log = logManager.GetLogger(nameof(GroupMappingEpisodeMapper));
            _tvDbClient = tvDbClient;
        }

        public Task<Option<TvDbEpisodeData>> MapEpisodeAsync(int aniDbEpisodeIndex,
            EpisodeGroupMapping episodeGroupMapping, int tvDbSeriesId)
        {
            var episodeMapping = GetEpisodeMapping(aniDbEpisodeIndex, episodeGroupMapping);
            var tvDbEpisodeIndex =
                GetTvDbEpisodeIndex(aniDbEpisodeIndex, episodeGroupMapping.TvDbEpisodeIndexOffset,
                    episodeMapping);

            return GetTvDbEpisodeAsync(tvDbSeriesId, episodeGroupMapping.TvDbSeasonIndex, tvDbEpisodeIndex)
                .MatchAsync(tvDbEpisodeData =>
                {
                    _log.Debug(
                        $"Found mapped TvDb episode: {tvDbEpisodeData}");

                    return tvDbEpisodeData;
                }, () => Option<TvDbEpisodeData>.None);
        }

        private Option<EpisodeMapping> GetEpisodeMapping(int aniDbEpisodeIndex, EpisodeGroupMapping episodeGroupMapping)
        {
            var episodeMapping =
                episodeGroupMapping.EpisodeMappings?.FirstOrDefault(m => m.AniDbEpisodeIndex == aniDbEpisodeIndex);

            return episodeMapping;
        }

        private int GetTvDbEpisodeIndex(int aniDbEpisodeIndex, int tvDbEpisodeIndexOffset,
            Option<EpisodeMapping> episodeMapping)
        {
            return episodeMapping.Match(m => m.TvDbEpisodeIndex,
                () => aniDbEpisodeIndex + tvDbEpisodeIndexOffset);
        }

        private async Task<Option<TvDbEpisodeData>> GetTvDbEpisodeAsync(int tvDbSeriesId, int seasonIndex,
            int episodeIndex)
        {
            var episodes = await _tvDbClient.GetEpisodesAsync(tvDbSeriesId);

            return episodes.Match(ec =>
                    ec.Find(e => e.AiredSeason == seasonIndex && e.AiredEpisodeNumber == episodeIndex),
                () => Option<TvDbEpisodeData>.None);
        }
    }
}