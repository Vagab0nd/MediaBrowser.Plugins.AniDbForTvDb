using System.Linq;
using Functional.Maybe;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.Anime.AniDb.Series.Data;

namespace MediaBrowser.Plugins.Anime.AniDb.Mapping
{
    public class AniDbMapper : IAniDbMapper
    {
        private readonly IMappingList _aniDbMappingList;
        private readonly ILogger _log;

        public AniDbMapper(IMappingList aniDbMappingList, ILogManager logManager)
        {
            _aniDbMappingList = aniDbMappingList;
            _log = logManager.GetLogger(nameof(AniDbMapper));
        }

        public Maybe<SeriesIds> GetMappedSeriesIds(int aniDbSeriesId)
        {
            var mapping = _aniDbMappingList.GetSeriesMapping(aniDbSeriesId);

            return mapping.Select(m => m.Ids);
        }

        public MappedEpisodeResult GetMappedTvDbEpisodeId(int aniDbSeriesId, IAniDbEpisodeNumber aniDbEpisodeNumber)
        {
            var result = new MappedEpisodeResult(new UnmappedEpisodeNumber());

            if (aniDbEpisodeNumber == null)
            {
                return result;
            }

            var seriesMapping = _aniDbMappingList.GetSeriesMapping(aniDbSeriesId);

            seriesMapping.Match(sm =>
                {
                    var episodeMapping = sm.GetEpisodeGroupMapping(aniDbEpisodeNumber);
                    var followingTvDbEpisodeNumber = sm.GetSpecialEpisodePosition(aniDbEpisodeNumber)
                        .Select(p => GetMappedTvDbEpisodeId(aniDbSeriesId,
                            new EpisodeNumberData
                            {
                                RawNumber = p.FollowingStandardEpisodeIndex.ToString(),
                                RawType = 1
                            }).Match(
                            tvDbEpisodeNumber => tvDbEpisodeNumber.ToMaybe(),
                            absolute => Maybe<TvDbEpisodeNumber>.Nothing,
                            unmapped => Maybe<TvDbEpisodeNumber>.Nothing)).Collapse();

                    episodeMapping.Match(
                        m =>
                        {
                            var tvDbEpisodeNumber =
                                GetTvDbEpisodeNumber(m, aniDbEpisodeNumber, followingTvDbEpisodeNumber);

                            _log.Debug(
                                $"Found mapped TvDb season index '{tvDbEpisodeNumber.SeasonIndex}', episode index '{tvDbEpisodeNumber.EpisodeIndex}'");

                            result = new MappedEpisodeResult(tvDbEpisodeNumber);
                        },
                        () => sm.DefaultTvDbSeason.Match(
                            tvDbSeason =>
                            {
                                var tvDbEpisodeNumber = new TvDbEpisodeNumber(tvDbSeason.Index,
                                    aniDbEpisodeNumber.Number + sm.DefaultTvDbEpisodeIndexOffset,
                                    followingTvDbEpisodeNumber);

                                _log.Debug(
                                    $"Found mapped TvDb season index '{tvDbEpisodeNumber.SeasonIndex}', episode index '{tvDbEpisodeNumber.EpisodeIndex}'");

                                result = new MappedEpisodeResult(tvDbEpisodeNumber);
                            },
                            absoluteTvDbSeason =>
                            {
                                var absoluteEpisodeNumber = new AbsoluteEpisodeNumber(aniDbEpisodeNumber.Number);

                                _log.Debug(
                                    $"Found mapped absolute TvDb episode index '{absoluteEpisodeNumber.EpisodeIndex}'");

                                result = new MappedEpisodeResult(absoluteEpisodeNumber);
                            }));
                },
                () => _log.Debug(
                    $"Failed to find mapped TvDb episode index for AniDb series Id '{aniDbSeriesId}', episode index '{aniDbEpisodeNumber.Number}'"));

            return result;
        }

        private TvDbEpisodeNumber GetTvDbEpisodeNumber(EpisodeGroupMapping episodeGroupMapping,
            IAniDbEpisodeNumber aniDbEpisodeNumber, Maybe<TvDbEpisodeNumber> followingTvDbEpisodeNumber)
        {
            var episodeMapping =
                episodeGroupMapping.EpisodeMappings?.FirstOrDefault(m => m.AniDbEpisodeIndex ==
                    aniDbEpisodeNumber.Number);

            var tvDbEpisodeIndex = episodeMapping?.TvDbEpisodeIndex ??
                aniDbEpisodeNumber.Number + episodeGroupMapping.TvDbEpisodeIndexOffset;

            return new TvDbEpisodeNumber(episodeGroupMapping.TvDbSeasonIndex, tvDbEpisodeIndex,
                followingTvDbEpisodeNumber);
        }
    }
}