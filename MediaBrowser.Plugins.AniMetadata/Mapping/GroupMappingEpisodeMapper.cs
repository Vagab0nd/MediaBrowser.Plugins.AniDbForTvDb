using System.Linq;
using LanguageExt;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.Mapping
{
    /// <summary>
    ///     Maps an AniDb episode to a TvDb episode (and vice versa) using an <see cref="EpisodeGroupMapping" />
    /// </summary>
    internal class GroupMappingEpisodeMapper : IGroupMappingEpisodeMapper
    {
        private readonly IAniDbClient _aniDbClient;
        private readonly ILogger _log;
        private readonly ITvDbClient _tvDbClient;

        public GroupMappingEpisodeMapper(ITvDbClient tvDbClient, IAniDbClient aniDbClient, ILogManager logManager)
        {
            _log = logManager.GetLogger(nameof(GroupMappingEpisodeMapper));
            _tvDbClient = tvDbClient;
            _aniDbClient = aniDbClient;
        }

        public OptionAsync<TvDbEpisodeData> MapAniDbEpisodeAsync(int aniDbEpisodeIndex,
            EpisodeGroupMapping episodeGroupMapping, int tvDbSeriesId)
        {
            var episodeMapping =
                episodeGroupMapping.EpisodeMappings?.FirstOrDefault(m => m.AniDbEpisodeIndex == aniDbEpisodeIndex);

            var tvDbEpisodeIndex =
                GetTvDbEpisodeIndex(aniDbEpisodeIndex, episodeGroupMapping.TvDbEpisodeIndexOffset,
                    episodeMapping);

            return GetTvDbEpisodeAsync(tvDbSeriesId, episodeGroupMapping.TvDbSeasonIndex, tvDbEpisodeIndex)
                .Map(tvDbEpisodeData =>
                {
                    _log.Debug($"Found mapped TvDb episode: {tvDbEpisodeData}");

                    return tvDbEpisodeData;
                });
        }

        public OptionAsync<AniDbEpisodeData> MapTvDbEpisodeAsync(int tvDbEpisodeIndex,
            EpisodeGroupMapping episodeGroupMapping, int aniDbSeriesId)
        {
            var episodeMapping = GetTvDbEpisodeMapping(tvDbEpisodeIndex, episodeGroupMapping);

            var aniDbEpisodeIndex =
                GetAniDbEpisodeIndex(tvDbEpisodeIndex, episodeGroupMapping.TvDbEpisodeIndexOffset,
                    episodeMapping);

            return GetAniDbEpisodeAsync(aniDbSeriesId, episodeGroupMapping.AniDbSeasonIndex, aniDbEpisodeIndex)
                .Map(aniDbEpisodeData =>
                {
                    _log.Debug(
                        $"Found mapped AniDb episode: {aniDbEpisodeData}");

                    return aniDbEpisodeData;
                });
        }

        private Option<EpisodeMapping> GetTvDbEpisodeMapping(int tvDbEpisodeIndex,
            EpisodeGroupMapping episodeGroupMapping)
        {
            var episodeMapping =
                episodeGroupMapping.EpisodeMappings?.FirstOrDefault(m => m.TvDbEpisodeIndex == tvDbEpisodeIndex);

            return episodeMapping;
        }

        private int GetTvDbEpisodeIndex(int aniDbEpisodeIndex, int tvDbEpisodeIndexOffset,
            Option<EpisodeMapping> episodeMapping)
        {
            return episodeMapping.Match(m => m.TvDbEpisodeIndex,
                () => aniDbEpisodeIndex + tvDbEpisodeIndexOffset);
        }

        private int GetAniDbEpisodeIndex(int tvDbEpisodeIndex, int tvDbEpisodeIndexOffset,
            Option<EpisodeMapping> episodeMapping)
        {
            return episodeMapping.Match(m => m.AniDbEpisodeIndex,
                () => tvDbEpisodeIndex - tvDbEpisodeIndexOffset);
        }

        private OptionAsync<TvDbEpisodeData> GetTvDbEpisodeAsync(int tvDbSeriesId, int seasonIndex,
            int episodeIndex)
        {
            return _tvDbClient.GetEpisodesAsync(tvDbSeriesId)
                .MapAsync(episodes =>
                    episodes.Find(e => e.AiredSeason == seasonIndex && e.AiredEpisodeNumber == episodeIndex));
        }

        private OptionAsync<AniDbEpisodeData> GetAniDbEpisodeAsync(int aniDbSeriesId, int seasonIndex,
            int episodeIndex)
        {
            return _aniDbClient.GetSeriesAsync(aniDbSeriesId)
                .BindAsync(aniDbSeries =>
                    aniDbSeries.Episodes.Find(e =>
                        e.EpisodeNumber.SeasonNumber == seasonIndex && e.EpisodeNumber.Number == episodeIndex));
        }
    }
}