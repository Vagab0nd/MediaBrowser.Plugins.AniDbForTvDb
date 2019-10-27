using System.Threading.Tasks;
using Emby.AniDbMetaStructure.TvDb;
using Emby.AniDbMetaStructure.TvDb.Data;
using LanguageExt;
using MediaBrowser.Model.Logging;

namespace Emby.AniDbMetaStructure.Mapping
{
    /// <summary>
    ///     Maps an AniDb episode to a TvDb episode using a default season
    /// </summary>
    internal class DefaultSeasonEpisodeMapper : IDefaultSeasonEpisodeMapper
    {
        private readonly ILogger log;
        private readonly ITvDbClient tvDbClient;

        public DefaultSeasonEpisodeMapper(ITvDbClient tvDbClient, ILogManager logManager)
        {
            this.log = logManager.GetLogger(nameof(DefaultSeasonEpisodeMapper));
            this.tvDbClient = tvDbClient;
        }

        public Task<Option<TvDbEpisodeData>> MapEpisodeAsync(int aniDbEpisodeIndex,
            ISeriesMapping seriesMapping)
        {
            return seriesMapping.Ids.TvDbSeriesId.MatchAsync(tvDbSeriesId =>
                    seriesMapping.DefaultTvDbSeason.Match(
                        tvDbSeason =>
                            this.MapEpisodeWithDefaultSeasonAsync(aniDbEpisodeIndex, tvDbSeriesId,
                                seriesMapping.DefaultTvDbEpisodeIndexOffset, tvDbSeason.Index),
                        absoluteTvDbSeason =>
                            this.MapEpisodeViaAbsoluteEpisodeIndexAsync(aniDbEpisodeIndex, tvDbSeriesId)),
                () =>
                {
                    this.log.Debug($"Failed to map AniDb episode {aniDbEpisodeIndex}");
                    return Option<TvDbEpisodeData>.None;
                });
        }

        private async Task<Option<TvDbEpisodeData>> MapEpisodeWithDefaultSeasonAsync(int aniDbEpisodeIndex,
            int tvDbSeriesId, int defaultTvDbEpisodeIndexOffset, int defaultTvDbSeasonIndex)
        {
            var tvDbEpisodeIndex = aniDbEpisodeIndex + defaultTvDbEpisodeIndexOffset;

            var tvDbEpisodeData =
                await this.tvDbClient.GetEpisodeAsync(tvDbSeriesId, defaultTvDbSeasonIndex, tvDbEpisodeIndex);

            return tvDbEpisodeData.Match(d =>
            {
                this.log.Debug(
                    $"Found mapped TvDb episode: {tvDbEpisodeData}");

                return d;
            }, () => Option<TvDbEpisodeData>.None);
        }

        private Task<Option<TvDbEpisodeData>> MapEpisodeViaAbsoluteEpisodeIndexAsync(int aniDbEpisodeIndex,
            int tvDbSeriesId)
        {
            return this.tvDbClient.GetEpisodeAsync(tvDbSeriesId, aniDbEpisodeIndex)
                .MatchAsync(tvDbEpisodeData =>
                {
                    this.log.Debug(
                        $"Found mapped TvDb episode via absolute episode index: {tvDbEpisodeData}");

                    return tvDbEpisodeData;
                }, () => Option<TvDbEpisodeData>.None);
        }
    }
}