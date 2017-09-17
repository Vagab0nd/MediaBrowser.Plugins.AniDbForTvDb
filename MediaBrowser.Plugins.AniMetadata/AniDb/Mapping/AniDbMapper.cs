using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.Mapping
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

        public Option<SeriesIds> GetMappedSeriesIdsFromAniDb(int aniDbSeriesId)
        {
            var mapping = _aniDbMappingList.GetSeriesMappingFromAniDb(aniDbSeriesId);

            return mapping.Select(m => m.Ids);
        }

        public Option<SeriesIds> GetMappedSeriesIdsFromTvDb(int tvDbSeriesId)
        {
            var mapping = _aniDbMappingList.GetSeriesMappingFromTvDb(tvDbSeriesId);

            return mapping.Select(m => m.Ids);
        }

        public Task<MappedEpisodeResult> GetMappedTvDbEpisodeIdAsync(int aniDbSeriesId,
            IAniDbEpisodeNumber aniDbEpisodeNumber)
        {
            var emptyResult = Task.FromResult((MappedEpisodeResult)new UnmappedEpisodeNumber());

            if (aniDbEpisodeNumber == null)
            {
                return emptyResult;
            }

            var seriesMapping = _aniDbMappingList.GetSeriesMappingFromAniDb(aniDbSeriesId);

            var result = seriesMapping.Match(sm =>
                {
                    var episodeGroupMapping = sm.GetEpisodeGroupMapping(aniDbEpisodeNumber);
                    var followingTvDbEpisodeNumber =
                        GetFollowingTvDbEpisodeNumberAsync(aniDbSeriesId, sm, aniDbEpisodeNumber).Result;

                    return episodeGroupMapping.Match(
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
            SeriesMapping seriesMapping, Option<TvDbEpisodeNumber> followingTvDbEpisodeNumber)
        {
            return await seriesMapping.DefaultTvDbSeason.Match(
                tvDbSeason =>
                    MapEpisodeToDefaultSeasonAsync(aniDbEpisodeNumber, seriesMapping, followingTvDbEpisodeNumber,
                        tvDbSeason),
                absoluteTvDbSeason =>
                    MapEpisodeToAbsoluteEpisodeIndexAsync(aniDbEpisodeNumber, seriesMapping.Ids.TvDbSeriesId));
        }

        private async Task<MappedEpisodeResult> MapEpisodeToAbsoluteEpisodeIndexAsync(
            IAniDbEpisodeNumber aniDbEpisodeNumber, Option<int> tvDbSeriesId)
        {
            var tvDbEpisodeId = await tvDbSeriesId.Match(
                id => GetTvDbEpisodeIdAsync(id, aniDbEpisodeNumber.Number),
                () => Task.FromResult(Option<int>.None));
            var absoluteEpisodeNumber = new AbsoluteEpisodeNumber(tvDbEpisodeId, aniDbEpisodeNumber.Number);

            _log.Debug(
                $"Found mapped absolute TvDb episode index '{absoluteEpisodeNumber.EpisodeIndex}'");

            return absoluteEpisodeNumber;
        }

        private async Task<MappedEpisodeResult> MapEpisodeToDefaultSeasonAsync(IAniDbEpisodeNumber aniDbEpisodeNumber,
            SeriesMapping seriesMapping, Option<TvDbEpisodeNumber> followingTvDbEpisodeNumber,
            TvDbSeason defaultTvDbSeason)
        {
            var tvDbSeasonIndex = defaultTvDbSeason.Index;
            var tvDbEpisodeIndex = aniDbEpisodeNumber.Number + seriesMapping.DefaultTvDbEpisodeIndexOffset;
            var tvDbEpisodeId = await seriesMapping.Ids.TvDbSeriesId.Match(
                id => GetTvDbEpisodeIdAsync(id, tvDbSeasonIndex, tvDbEpisodeIndex),
                () => Task.FromResult(Option<int>.None));

            var tvDbEpisodeNumber = new TvDbEpisodeNumber(tvDbEpisodeId, tvDbSeasonIndex, tvDbEpisodeIndex,
                followingTvDbEpisodeNumber);

            _log.Debug(
                $"Found mapped TvDb season index '{tvDbEpisodeNumber.SeasonIndex}', episode index '{tvDbEpisodeNumber.EpisodeIndex}'");

            return tvDbEpisodeNumber;
        }

        private async Task<MappedEpisodeResult> MapEpisodeUsingGroupMappingAsync(IAniDbEpisodeNumber aniDbEpisodeNumber,
            EpisodeGroupMapping episodeGroupMapping, Option<TvDbEpisodeNumber> followingTvDbEpisodeNumber,
            Option<int> tvDbSeriesId)
        {
            var episodeMapping = GetEpisodeMapping(aniDbEpisodeNumber, episodeGroupMapping);
            var tvDbEpisodeIndex = GetTvDbEpisodeIndex(aniDbEpisodeNumber, episodeGroupMapping, episodeMapping);
            var tvDbEpisodeId = await tvDbSeriesId.Match(
                id => GetTvDbEpisodeIdAsync(id, episodeGroupMapping.TvDbSeasonIndex, tvDbEpisodeIndex),
                () => Task.FromResult(Option<int>.None));

            var tvDbEpisodeNumber = new TvDbEpisodeNumber(tvDbEpisodeId, episodeGroupMapping.TvDbSeasonIndex,
                tvDbEpisodeIndex,
                followingTvDbEpisodeNumber);

            _log.Debug(
                $"Found mapped TvDb season index '{tvDbEpisodeNumber.SeasonIndex}', episode index '{tvDbEpisodeNumber.EpisodeIndex}'");

            return tvDbEpisodeNumber;
        }

        private Option<EpisodeMapping> GetEpisodeMapping(IAniDbEpisodeNumber aniDbEpisodeNumber,
            EpisodeGroupMapping episodeGroupMapping)
        {
            var episodeMapping =
                episodeGroupMapping.EpisodeMappings?.FirstOrDefault(m =>
                    m.AniDbEpisodeIndex == aniDbEpisodeNumber.Number);

            return episodeMapping;
        }

        private int GetTvDbEpisodeIndex(IAniDbEpisodeNumber aniDbEpisodeNumber,
            EpisodeGroupMapping episodeGroupMapping, Option<EpisodeMapping> episodeMapping)
        {
            return episodeMapping.Match(m => m.TvDbEpisodeIndex,
                () => aniDbEpisodeNumber.Number + episodeGroupMapping.TvDbEpisodeIndexOffset);
        }

        private async Task<Option<TvDbEpisodeNumber>> GetFollowingTvDbEpisodeNumberAsync(int aniDbSeriesId,
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

            return mappedTvDbEpisodeId.Match(e => e.Match(
                    tvDbEpisodeNumber => tvDbEpisodeNumber,
                    absolute => Option<TvDbEpisodeNumber>.None,
                    unmapped => Option<TvDbEpisodeNumber>.None),
                () => Option<TvDbEpisodeNumber>.None);
        }

        private async Task<Option<int>> GetTvDbEpisodeIdAsync(int tvDbSeriesId, int seasonIndex, int episodeIndex)
        {
            var episodes = await _tvDbClient.GetEpisodesAsync(tvDbSeriesId);

            return episodes.Match(ec =>
                        ec.Find(e => e.AiredSeason == seasonIndex && e.AiredEpisodeNumber == episodeIndex),
                    () => Option<TvDbEpisodeData>.None)
                .Select(e => e.Id);
        }

        private async Task<Option<int>> GetTvDbEpisodeIdAsync(int tvDbSeriesId, int absoluteEpisodeIndex)
        {
            var episodes = await _tvDbClient.GetEpisodesAsync(tvDbSeriesId);

            return episodes.Match(ec => ((IEnumerable<TvDbEpisodeData>)ec).Find(e =>
                        e.AbsoluteNumber.Match(index => index == absoluteEpisodeIndex, () => false))
                    .Bind<int>(e => e.Id),
                () => Option<int>.None);
        }
    }
}