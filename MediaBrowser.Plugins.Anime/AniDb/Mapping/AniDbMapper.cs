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
            var emptyResult = new MappedEpisodeResult(new UnmappedEpisodeNumber());

            if (aniDbEpisodeNumber == null)
            {
                return emptyResult;
            }

            var seriesMapping = _aniDbMappingList.GetSeriesMapping(aniDbSeriesId);

            var result = seriesMapping.SelectOrElse(sm =>
                {
                    var episodeGroupMapping = sm.GetEpisodeGroupMapping(aniDbEpisodeNumber);
                    var followingTvDbEpisodeNumber =
                        GetFollowingTvDbEpisodeNumber(aniDbSeriesId, sm, aniDbEpisodeNumber);

                    return episodeGroupMapping.SelectOrElse(
                        m => MapEpisodeUsingGroupMapping(aniDbEpisodeNumber, m, followingTvDbEpisodeNumber),
                        () => MapEpisodeToDefaultSeason(aniDbEpisodeNumber, sm, followingTvDbEpisodeNumber));
                },
                () =>
                {
                    _log.Debug(
                        $"Failed to find mapped TvDb episode index for AniDb series Id '{aniDbSeriesId}', episode index '{aniDbEpisodeNumber.Number}'");
                    return emptyResult;
                });

            return result;
        }

        private MappedEpisodeResult MapEpisodeToDefaultSeason(IAniDbEpisodeNumber aniDbEpisodeNumber,
            SeriesMapping seriesMapping, Maybe<TvDbEpisodeNumber> followingTvDbEpisodeNumber)
        {
            return seriesMapping.DefaultTvDbSeason.Match(
                tvDbSeason =>
                    MapEpisodeToDefaultSeason(aniDbEpisodeNumber, seriesMapping, followingTvDbEpisodeNumber,
                        tvDbSeason),
                absoluteTvDbSeason => MapEpisodeToAbsoluteEpisodeIndex(aniDbEpisodeNumber));
        }

        private MappedEpisodeResult MapEpisodeToAbsoluteEpisodeIndex(IAniDbEpisodeNumber aniDbEpisodeNumber)
        {
            var absoluteEpisodeNumber = new AbsoluteEpisodeNumber(aniDbEpisodeNumber.Number);

            _log.Debug(
                $"Found mapped absolute TvDb episode index '{absoluteEpisodeNumber.EpisodeIndex}'");

            return new MappedEpisodeResult(absoluteEpisodeNumber);
        }

        private MappedEpisodeResult MapEpisodeToDefaultSeason(IAniDbEpisodeNumber aniDbEpisodeNumber,
            SeriesMapping seriesMapping, Maybe<TvDbEpisodeNumber> followingTvDbEpisodeNumber,
            TvDbSeason defaultTvDbSeason)
        {
            var tvDbEpisodeNumber = new TvDbEpisodeNumber(defaultTvDbSeason.Index,
                aniDbEpisodeNumber.Number + seriesMapping.DefaultTvDbEpisodeIndexOffset,
                followingTvDbEpisodeNumber);

            _log.Debug(
                $"Found mapped TvDb season index '{tvDbEpisodeNumber.SeasonIndex}', episode index '{tvDbEpisodeNumber.EpisodeIndex}'");

            return new MappedEpisodeResult(tvDbEpisodeNumber);
        }

        private MappedEpisodeResult MapEpisodeUsingGroupMapping(IAniDbEpisodeNumber aniDbEpisodeNumber,
            EpisodeGroupMapping episodeGroupMapping, Maybe<TvDbEpisodeNumber> followingTvDbEpisodeNumber)
        {
            var episodeMapping = GetEpisodeMapping(aniDbEpisodeNumber, episodeGroupMapping);
            var tvDbEpisodeIndex = GetTvDbEpisodeIndex(aniDbEpisodeNumber, episodeGroupMapping, episodeMapping);

            var tvDbEpisodeNumber = new TvDbEpisodeNumber(episodeGroupMapping.TvDbSeasonIndex, tvDbEpisodeIndex,
                followingTvDbEpisodeNumber);

            _log.Debug(
                $"Found mapped TvDb season index '{tvDbEpisodeNumber.SeasonIndex}', episode index '{tvDbEpisodeNumber.EpisodeIndex}'");

            return new MappedEpisodeResult(tvDbEpisodeNumber);
        }

        private Maybe<EpisodeMapping> GetEpisodeMapping(IAniDbEpisodeNumber aniDbEpisodeNumber,
            EpisodeGroupMapping episodeGroupMapping)
        {
            var episodeMapping =
                episodeGroupMapping.EpisodeMappings?.FirstOrDefault(m =>
                    m.AniDbEpisodeIndex == aniDbEpisodeNumber.Number);

            return episodeMapping.ToMaybe();
        }

        private int GetTvDbEpisodeIndex(IAniDbEpisodeNumber aniDbEpisodeNumber,
            EpisodeGroupMapping episodeGroupMapping, Maybe<EpisodeMapping> episodeMapping)
        {
            return episodeMapping.SelectOrElse(m => m.TvDbEpisodeIndex,
                () => aniDbEpisodeNumber.Number + episodeGroupMapping.TvDbEpisodeIndexOffset);
        }

        private Maybe<TvDbEpisodeNumber> GetFollowingTvDbEpisodeNumber(int aniDbSeriesId, SeriesMapping seriesMapping,
            IAniDbEpisodeNumber aniDbEpisodeNumber)
        {
            return seriesMapping.GetSpecialEpisodePosition(aniDbEpisodeNumber)
                .Select(p => GetMappedTvDbEpisodeId(aniDbSeriesId,
                        new EpisodeNumberData
                        {
                            RawNumber = p.FollowingStandardEpisodeIndex.ToString(),
                            RawType = 1
                        })
                    .Match(
                        tvDbEpisodeNumber => tvDbEpisodeNumber.ToMaybe(),
                        absolute => Maybe<TvDbEpisodeNumber>.Nothing,
                        unmapped => Maybe<TvDbEpisodeNumber>.Nothing))
                .Collapse();
        }
    }
}