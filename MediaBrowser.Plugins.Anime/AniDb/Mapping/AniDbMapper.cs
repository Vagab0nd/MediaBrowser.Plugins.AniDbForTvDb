using System.Linq;
using System.Threading.Tasks;
using Functional.Maybe;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.Anime.AniDb.Series.Data;
using MediaBrowser.Plugins.Anime.TvDb;

namespace MediaBrowser.Plugins.Anime.AniDb.Mapping
{
    public class AniDbMapper : IAniDbMapper
    {
        private readonly IMappingList _aniDbMappingList;
        private readonly ILogger _log;
        private readonly ITvDbClient _tvDbClient;

        public AniDbMapper(IMappingList aniDbMappingList, ITvDbClient tvDbClient, ILogManager logManager)
        {
            _aniDbMappingList = aniDbMappingList;
            _tvDbClient = tvDbClient;
            _log = logManager.GetLogger(nameof(AniDbMapper));
        }

        public Maybe<SeriesIds> GetMappedSeriesIds(int aniDbSeriesId)
        {
            var mapping = _aniDbMappingList.GetSeriesMapping(aniDbSeriesId);

            return mapping.Select(m => m.Ids);
        }

        public Task<MappedEpisodeResult> GetMappedTvDbEpisodeIdAsync(int aniDbSeriesId,
            IAniDbEpisodeNumber aniDbEpisodeNumber)
        {
            var emptyResult = Task.FromResult(new MappedEpisodeResult(new UnmappedEpisodeNumber()));

            if (aniDbEpisodeNumber == null)
            {
                return emptyResult;
            }

            var seriesMapping = _aniDbMappingList.GetSeriesMapping(aniDbSeriesId);

            var result = seriesMapping.SelectOrElse(sm =>
                {
                    var episodeGroupMapping = sm.GetEpisodeGroupMapping(aniDbEpisodeNumber);
                    var followingTvDbEpisodeNumber =
                        GetFollowingTvDbEpisodeNumberAsync(aniDbSeriesId, sm, aniDbEpisodeNumber).Result;

                    return episodeGroupMapping.SelectOrElse(
                        m => MapEpisodeUsingGroupMappingAsync(aniDbEpisodeNumber, m, followingTvDbEpisodeNumber,
                            sm.Ids.TvDbSeriesId),
                        () => MapEpisodeToDefaultSeasonAsync(aniDbEpisodeNumber, sm, followingTvDbEpisodeNumber));
                },
                () =>
                {
                    _log.Debug(
                        $"Failed to find mapped TvDb episode index for AniDb series Id '{aniDbSeriesId}', episode index '{aniDbEpisodeNumber.Number}'");
                    return emptyResult;
                });

            return result;
        }

        private async Task<MappedEpisodeResult> MapEpisodeToDefaultSeasonAsync(IAniDbEpisodeNumber aniDbEpisodeNumber,
            SeriesMapping seriesMapping, Maybe<TvDbEpisodeNumber> followingTvDbEpisodeNumber)
        {
            return await seriesMapping.DefaultTvDbSeason.Match(
                tvDbSeason =>
                    MapEpisodeToDefaultSeasonAsync(aniDbEpisodeNumber, seriesMapping, followingTvDbEpisodeNumber,
                        tvDbSeason),
                absoluteTvDbSeason =>
                    MapEpisodeToAbsoluteEpisodeIndexAsync(aniDbEpisodeNumber, seriesMapping.Ids.TvDbSeriesId));
        }

        private async Task<MappedEpisodeResult> MapEpisodeToAbsoluteEpisodeIndexAsync(
            IAniDbEpisodeNumber aniDbEpisodeNumber, Maybe<int> tvDbSeriesId)
        {
            var tvDbEpisodeId = await tvDbSeriesId.Select(id => GetTvDbEpisodeIdAsync(id, aniDbEpisodeNumber.Number));
            var absoluteEpisodeNumber = new AbsoluteEpisodeNumber(tvDbEpisodeId, aniDbEpisodeNumber.Number);

            _log.Debug(
                $"Found mapped absolute TvDb episode index '{absoluteEpisodeNumber.EpisodeIndex}'");

            return new MappedEpisodeResult(absoluteEpisodeNumber);
        }

        private async Task<MappedEpisodeResult> MapEpisodeToDefaultSeasonAsync(IAniDbEpisodeNumber aniDbEpisodeNumber,
            SeriesMapping seriesMapping, Maybe<TvDbEpisodeNumber> followingTvDbEpisodeNumber,
            TvDbSeason defaultTvDbSeason)
        {
            var tvDbSeasonIndex = defaultTvDbSeason.Index;
            var tvDbEpisodeIndex = aniDbEpisodeNumber.Number + seriesMapping.DefaultTvDbEpisodeIndexOffset;
            var tvDbEpisodeId =
                await seriesMapping.Ids.TvDbSeriesId.Select(id =>
                    GetTvDbEpisodeIdAsync(id, tvDbSeasonIndex, tvDbEpisodeIndex));

            var tvDbEpisodeNumber = new TvDbEpisodeNumber(tvDbEpisodeId, tvDbSeasonIndex, tvDbEpisodeIndex,
                followingTvDbEpisodeNumber);

            _log.Debug(
                $"Found mapped TvDb season index '{tvDbEpisodeNumber.SeasonIndex}', episode index '{tvDbEpisodeNumber.EpisodeIndex}'");

            return new MappedEpisodeResult(tvDbEpisodeNumber);
        }

        private async Task<MappedEpisodeResult> MapEpisodeUsingGroupMappingAsync(IAniDbEpisodeNumber aniDbEpisodeNumber,
            EpisodeGroupMapping episodeGroupMapping, Maybe<TvDbEpisodeNumber> followingTvDbEpisodeNumber,
            Maybe<int> tvDbSeriesId)
        {
            var episodeMapping = GetEpisodeMapping(aniDbEpisodeNumber, episodeGroupMapping);
            var tvDbEpisodeIndex = GetTvDbEpisodeIndex(aniDbEpisodeNumber, episodeGroupMapping, episodeMapping);
            var tvDbEpisodeId =
                await tvDbSeriesId.Select(id =>
                    GetTvDbEpisodeIdAsync(id, episodeGroupMapping.TvDbSeasonIndex, tvDbEpisodeIndex));

            var tvDbEpisodeNumber = new TvDbEpisodeNumber(tvDbEpisodeId, episodeGroupMapping.TvDbSeasonIndex,
                tvDbEpisodeIndex,
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

        private async Task<Maybe<TvDbEpisodeNumber>> GetFollowingTvDbEpisodeNumberAsync(int aniDbSeriesId,
            SeriesMapping seriesMapping,
            IAniDbEpisodeNumber aniDbEpisodeNumber)
        {
            var mappedTvDbEpisodeId = await seriesMapping.GetSpecialEpisodePosition(aniDbEpisodeNumber)
                .Select(p => GetMappedTvDbEpisodeIdAsync(aniDbSeriesId,
                    new EpisodeNumberData
                    {
                        RawNumber = p.FollowingStandardEpisodeIndex.ToString(),
                        RawType = 1
                    }));

            return mappedTvDbEpisodeId.Select(e => e.Match(
                tvDbEpisodeNumber => tvDbEpisodeNumber.ToMaybe(),
                absolute => Maybe<TvDbEpisodeNumber>.Nothing,
                unmapped => Maybe<TvDbEpisodeNumber>.Nothing));
        }

        private async Task<Maybe<int>> GetTvDbEpisodeIdAsync(int tvDbSeriesId, int seasonIndex, int episodeIndex)
        {
            var episodes = await _tvDbClient.GetEpisodesAsync(tvDbSeriesId);

            return episodes.Select(ec =>
                    ec.FirstMaybe(e => e.AiredSeason == seasonIndex && e.AiredEpisodeNumber == episodeIndex))
                .Collapse()
                .Select(e => e.Id);
        }

        private async Task<Maybe<int>> GetTvDbEpisodeIdAsync(int tvDbSeriesId, int absoluteEpisodeIndex)
        {
            var episodes = await _tvDbClient.GetEpisodesAsync(tvDbSeriesId);

            return episodes.Select(ec =>
                    ec.FirstMaybe(e => e.AbsoluteNumber.SelectOrElse(index => index == absoluteEpisodeIndex, () => false))
                .Select(e => e.Id));
        }
    }
}