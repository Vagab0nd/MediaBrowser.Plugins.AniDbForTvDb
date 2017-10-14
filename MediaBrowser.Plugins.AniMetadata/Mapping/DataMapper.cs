using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Providers;
using MediaBrowser.Plugins.AniMetadata.Providers.AniDb;
using MediaBrowser.Plugins.AniMetadata.TvDb;

namespace MediaBrowser.Plugins.AniMetadata.Mapping
{
    internal class DataMapper : IDataMapper
    {
        private readonly IMappingList _mappingList;
        private readonly IEpisodeMapper _episodeMapper;
        private readonly IEpisodeMatcher _episodeMatcher;
        private readonly ILogger _log;
        private readonly ITvDbClient _tvDbClient;

        public DataMapper(IMappingList mappingList, ITvDbClient tvDbClient, IEpisodeMatcher episodeMatcher,
            IEpisodeMapper episodeMapper, ILogManager logManager)
        {
            _mappingList = mappingList;
            _tvDbClient = tvDbClient;
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

            return seriesMapping.MatchAsync(async sm =>
                {
                    var episodeGroupMapping = sm.GetEpisodeGroupMapping(aniDbEpisodeData.EpisodeNumber);

                    var followingTvDbEpisode =
                        await GetFollowingTvDbEpisodeAsync(aniDbSeriesData, sm, aniDbEpisodeData);

                    var tvDbEpisodeData = await _episodeMapper.MapEpisodeAsync(aniDbEpisodeData.EpisodeNumber.Number, sm,
                        episodeGroupMapping);

                    return tvDbEpisodeData.Match(
                        d => new CombinedEpisodeData(aniDbEpisodeData, d, followingTvDbEpisode),
                        () => (EpisodeData)new AniDbOnlyEpisodeData(aniDbEpisodeData));
                },
                () =>
                {
                    _log.Debug(
                        $"Failed to find mapped TvDb episode index for AniDb series Id '{aniDbSeriesData.Id}', episode index '{aniDbEpisodeData?.EpisodeNumber?.Number}'");
                    return new AniDbOnlyEpisodeData(aniDbEpisodeData);
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