using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.Mapping
{
    /// <summary>
    ///     Maps an AniDb episode to a TvDb episode using a default season
    /// </summary>
    internal class DefaultSeasonEpisodeMapper : IDefaultSeasonEpisodeMapper
    {
        private readonly ILogger _log;
        private readonly ITvDbClient _tvDbClient;

        public DefaultSeasonEpisodeMapper(ITvDbClient tvDbClient, ILogManager logManager)
        {
            _log = logManager.GetLogger(nameof(DefaultSeasonEpisodeMapper));
            _tvDbClient = tvDbClient;
        }

        public Task<Option<TvDbEpisodeData>> MapEpisodeAsync(int aniDbEpisodeIndex,
            ISeriesMapping seriesMapping)
        {
            return seriesMapping.Ids.TvDbSeriesId.MatchAsync(tvDbSeriesId =>
                    seriesMapping.DefaultTvDbSeason.Match(
                        tvDbSeason =>
                            MapEpisodeWithDefaultSeasonAsync(aniDbEpisodeIndex, tvDbSeriesId,
                                seriesMapping.DefaultTvDbEpisodeIndexOffset, tvDbSeason.Index),
                        absoluteTvDbSeason =>
                            MapEpisodeViaAbsoluteEpisodeIndexAsync(aniDbEpisodeIndex, tvDbSeriesId)),
                () =>
                {
                    _log.Debug($"Failed to map AniDb episode {aniDbEpisodeIndex}");
                    return Option<TvDbEpisodeData>.None;
                });
        }

        private async Task<Option<TvDbEpisodeData>> MapEpisodeWithDefaultSeasonAsync(int aniDbEpisodeIndex,
            int tvDbSeriesId, int defaultTvDbEpisodeIndexOffset, int defaultTvDbSeasonIndex)
        {
            var tvDbEpisodeIndex = aniDbEpisodeIndex + defaultTvDbEpisodeIndexOffset;

            var tvDbEpisodeData =
                await _tvDbClient.GetEpisodeAsync(tvDbSeriesId, defaultTvDbSeasonIndex, tvDbEpisodeIndex);

            return tvDbEpisodeData.Match(d =>
            {
                _log.Debug(
                    $"Found mapped TvDb episode: {tvDbEpisodeData}");

                return d;
            }, () => Option<TvDbEpisodeData>.None);
        }

        private Task<Option<TvDbEpisodeData>> MapEpisodeViaAbsoluteEpisodeIndexAsync(int aniDbEpisodeIndex,
            int tvDbSeriesId)
        {
            return _tvDbClient.GetEpisodeAsync(tvDbSeriesId, aniDbEpisodeIndex)
                .MatchAsync(tvDbEpisodeData =>
                {
                    _log.Debug(
                        $"Found mapped TvDb episode via absolute episode index: {tvDbEpisodeData}");

                    return tvDbEpisodeData;
                }, () => Option<TvDbEpisodeData>.None);
        }
    }
}