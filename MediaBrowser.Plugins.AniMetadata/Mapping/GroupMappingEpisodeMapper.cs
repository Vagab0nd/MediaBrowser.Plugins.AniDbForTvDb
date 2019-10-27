using System.Linq;
using Emby.AniDbMetaStructure.AniDb;
using Emby.AniDbMetaStructure.AniDb.SeriesData;
using Emby.AniDbMetaStructure.TvDb;
using Emby.AniDbMetaStructure.TvDb.Data;
using LanguageExt;
using MediaBrowser.Model.Logging;

namespace Emby.AniDbMetaStructure.Mapping
{
    /// <summary>
    ///     Maps an AniDb episode to a TvDb episode (and vice versa) using an <see cref="EpisodeGroupMapping" />
    /// </summary>
    internal class GroupMappingEpisodeMapper : IGroupMappingEpisodeMapper
    {
        private readonly IAniDbClient aniDbClient;
        private readonly ILogger log;
        private readonly ITvDbClient tvDbClient;

        public GroupMappingEpisodeMapper(ITvDbClient tvDbClient, IAniDbClient aniDbClient, ILogManager logManager)
        {
            this.log = logManager.GetLogger(nameof(GroupMappingEpisodeMapper));
            this.tvDbClient = tvDbClient;
            this.aniDbClient = aniDbClient;
        }

        public OptionAsync<TvDbEpisodeData> MapAniDbEpisodeAsync(int aniDbEpisodeIndex,
            EpisodeGroupMapping episodeGroupMapping, int tvDbSeriesId)
        {
            var episodeMapping =
                episodeGroupMapping.EpisodeMappings?.FirstOrDefault(m => m.AniDbEpisodeIndex == aniDbEpisodeIndex);

            var tvDbEpisodeIndex =
                this.GetTvDbEpisodeIndex(aniDbEpisodeIndex, episodeGroupMapping.TvDbEpisodeIndexOffset,
                    episodeMapping);

            return this.GetTvDbEpisodeAsync(tvDbSeriesId, episodeGroupMapping.TvDbSeasonIndex, tvDbEpisodeIndex)
                .Map(tvDbEpisodeData =>
                {
                    this.log.Debug($"Found mapped TvDb episode: {tvDbEpisodeData}");

                    return tvDbEpisodeData;
                });
        }

        public OptionAsync<AniDbEpisodeData> MapTvDbEpisodeAsync(int tvDbEpisodeIndex,
            EpisodeGroupMapping episodeGroupMapping, int aniDbSeriesId)
        {
            var episodeMapping = this.GetTvDbEpisodeMapping(tvDbEpisodeIndex, episodeGroupMapping);

            var aniDbEpisodeIndex =
                this.GetAniDbEpisodeIndex(tvDbEpisodeIndex, episodeGroupMapping.TvDbEpisodeIndexOffset,
                    episodeMapping);

            return this.GetAniDbEpisodeAsync(aniDbSeriesId, episodeGroupMapping.AniDbSeasonIndex, aniDbEpisodeIndex)
                .Map(aniDbEpisodeData =>
                {
                    this.log.Debug(
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
            return this.tvDbClient.GetEpisodesAsync(tvDbSeriesId)
                .MapAsync(episodes =>
                    episodes.Find(e => e.AiredSeason == seasonIndex && e.AiredEpisodeNumber == episodeIndex));
        }

        private OptionAsync<AniDbEpisodeData> GetAniDbEpisodeAsync(int aniDbSeriesId, int seasonIndex,
            int episodeIndex)
        {
            return this.aniDbClient.GetSeriesAsync(aniDbSeriesId)
                .BindAsync(aniDbSeries =>
                    aniDbSeries.Episodes.Find(e =>
                        e.EpisodeNumber.SeasonNumber == seasonIndex && e.EpisodeNumber.Number == episodeIndex));
        }
    }
}