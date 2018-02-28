using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Providers;
using MediaBrowser.Plugins.AniMetadata.Providers.AniDb;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.Mapping
{
    internal class DataMapper : IDataMapper
    {
        private readonly IAniDbClient _aniDbClient;
        private readonly IEpisodeMapper _episodeMapper;
        private readonly IEpisodeMatcher _episodeMatcher;
        private readonly ILogger _log;
        private readonly IMappingList _mappingList;
        private readonly ITvDbClient _tvDbClient;

        public DataMapper(IMappingList mappingList, ITvDbClient tvDbClient, IAniDbClient aniDbClient,
            IEpisodeMatcher episodeMatcher, IEpisodeMapper episodeMapper, ILogManager logManager)
        {
            _mappingList = mappingList;
            _tvDbClient = tvDbClient;
            _aniDbClient = aniDbClient;
            _episodeMatcher = episodeMatcher;
            _episodeMapper = episodeMapper;
            _log = logManager.GetLogger(nameof(DataMapper));
        }

        public Task<SeriesData> MapSeriesDataAsync(AniDbSeriesData aniDbSeriesData)
        {
            var defaultResult = (SeriesData)new AniDbOnlySeriesData(
                new SeriesIds(aniDbSeriesData.Id, Option<int>.None, Option<int>.None, Option<int>.None),
                aniDbSeriesData);

            return _mappingList.GetSeriesMappingFromAniDb(aniDbSeriesData.Id)
                .MatchAsync(m => m.Ids.TvDbSeriesId
                        .MatchAsync(tvDbSeriesId => _tvDbClient.GetSeriesAsync(tvDbSeriesId)
                                .MatchAsync(
                                    tvDbSeriesData => new CombinedSeriesData(m.Ids, aniDbSeriesData, tvDbSeriesData),
                                    () => defaultResult),
                            () => defaultResult),
                    () => defaultResult);
        }

        public Task<EpisodeData> MapEpisodeDataAsync(AniDbSeriesData aniDbSeriesData, AniDbEpisodeData aniDbEpisodeData)
        {
            var seriesMapping = _mappingList.GetSeriesMappingFromAniDb(aniDbSeriesData.Id);

            return seriesMapping.MatchAsync(sm =>
                {
                    var episodeGroupMapping = sm.GetEpisodeGroupMapping(aniDbEpisodeData.EpisodeNumber);

                    var followingTvDbEpisode = GetFollowingTvDbEpisodeAsync(aniDbSeriesData, sm, aniDbEpisodeData);

                    var tvDbEpisodeData = _episodeMapper.MapAniDbEpisodeAsync(aniDbEpisodeData.EpisodeNumber.Number,
                        sm, episodeGroupMapping);

                    return followingTvDbEpisode.Bind(following => tvDbEpisodeData.Match(
                        d => new CombinedEpisodeData(aniDbEpisodeData, d, following),
                        () => (EpisodeData)new AniDbOnlyEpisodeData(aniDbEpisodeData)));
                },
                () =>
                {
                    _log.Debug(
                        $"Failed to find mapped TvDb episode index for AniDb series Id '{aniDbSeriesData.Id}', episode index '{aniDbEpisodeData?.EpisodeNumber?.Number}'");
                    return new AniDbOnlyEpisodeData(aniDbEpisodeData);
                });
        }

        public Task<IEnumerable<SeriesData>> MapSeriesDataAsync(TvDbSeriesData tvDbSeriesData)
        {
            var defaultResult = (SeriesData)new NoSeriesData();

            return _mappingList.GetSeriesMappingsFromTvDb(tvDbSeriesData.Id)
                .MatchAsync(m =>
                    {
                        return Task.WhenAll(m.Select(series => _aniDbClient.GetSeriesAsync(series.Ids.AniDbSeriesId)
                                .MatchAsync(
                                    aniDbSeriesData =>
                                        new CombinedSeriesData(series.Ids, aniDbSeriesData, tvDbSeriesData),
                                    () => defaultResult)))
                            .Map(d => d.AsEnumerable());
                    },
                    () => Enumerable.Empty<SeriesData>());
        }

        public Task<EpisodeData> MapEpisodeDataAsync(int aniDbSeriesId, TvDbSeriesData tvDbSeriesData,
            TvDbEpisodeData tvDbEpisodeData)
        {
            var seriesMapping = _mappingList.GetSeriesMappingsFromTvDb(tvDbSeriesData.Id)
                .Map(sm => sm.Single(m => m.Ids.AniDbSeriesId == aniDbSeriesId));

            return seriesMapping.MatchAsync(sm =>
                {
                    var episodeGroupMapping = sm.GetEpisodeGroupMapping(tvDbEpisodeData.AiredEpisodeNumber,
                        tvDbEpisodeData.AiredSeason);

                    var aniDbEpisodeData = _episodeMapper.MapTvDbEpisodeAsync(tvDbEpisodeData.AiredEpisodeNumber,
                        sm,
                        episodeGroupMapping);

                    return aniDbEpisodeData.Match(
                        d => new CombinedEpisodeData(d, tvDbEpisodeData, new NoEpisodeData()),
                        () => (EpisodeData)new NoEpisodeData());
                },
                () =>
                {
                    _log.Debug(
                        $"Failed to find mapped AniDb episode index for TvDb series Id '{tvDbSeriesData.Id}', episode index '{tvDbEpisodeData?.AiredEpisodeNumber}'");
                    return new NoEpisodeData();
                });
        }

        private Task<EpisodeData> GetFollowingTvDbEpisodeAsync(AniDbSeriesData aniDbSeriesData,
            ISeriesMapping seriesMapping, AniDbEpisodeData aniDbEpisodeData)
        {
            return seriesMapping.GetSpecialEpisodePosition(aniDbEpisodeData.EpisodeNumber)
                .Bind(p =>
                    _episodeMatcher
                        .FindEpisode(aniDbSeriesData.Episodes, Option<int>.None, p.FollowingStandardEpisodeIndex,
                            Option<string>.None)
                )
                .MatchAsync(e => MapEpisodeDataAsync(aniDbSeriesData, e),
                    () => new NoEpisodeData());
        }
    }
}