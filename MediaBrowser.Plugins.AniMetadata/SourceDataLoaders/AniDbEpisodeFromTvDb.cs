using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Mapping;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.SourceDataLoaders
{
    /// <summary>
    ///     Loads AniDb episode data based on data from TvDb
    /// </summary>
    internal class AniDbEpisodeFromTvDb : ISourceDataLoader
    {
        private readonly IEpisodeMapper _episodeMapper;
        private readonly IMappingList _mappingList;
        private readonly ISources _sources;

        public AniDbEpisodeFromTvDb(ISources sources, IMappingList mappingList, IEpisodeMapper episodeMapper)
        {
            _sources = sources;
            _mappingList = mappingList;
            _episodeMapper = episodeMapper;
        }

        public bool CanLoadFrom(object sourceData)
        {
            return sourceData is ISourceData<TvDbEpisodeData>;
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IMediaItem mediaItem, object sourceData)
        {
            var tvDbSourceData = (ISourceData<TvDbEpisodeData>)sourceData;

            var resultContext = new ProcessResultContext(nameof(AniDbEpisodeFromTvDb), mediaItem.EmbyData.Identifier.Name,
                mediaItem.ItemType);

            var tvDbSeriesData = _sources.TvDb.GetSeriesData(mediaItem.EmbyData, resultContext);

            var tvDbEpisodeData = tvDbSourceData.Data;

            var aniDbSeriesId = mediaItem.EmbyData.GetParentId(MediaItemTypes.Series, _sources.AniDb)
                .ToEither(resultContext.Failed("Failed to find AniDb series Id"));

            var aniDbEpisodeData = tvDbSeriesData.BindAsync(seriesData =>
                aniDbSeriesId.BindAsync(id => MapEpisodeDataAsync(id, seriesData, tvDbEpisodeData, resultContext)));

            return aniDbEpisodeData.BindAsync(episodeData =>
                _sources.AniDb.SelectTitle(episodeData.Titles, mediaItem.EmbyData.Language, resultContext)
                    .Map(t => CreateSourceData(episodeData, t)));
        }

        private Task<Either<ProcessFailedResult, AniDbEpisodeData>> MapEpisodeDataAsync(int aniDbSeriesId,
            TvDbSeriesData tvDbSeriesData, TvDbEpisodeData tvDbEpisodeData, ProcessResultContext resultContext)
        {
            var seriesMapping = _mappingList.GetSeriesMappingsFromTvDb(tvDbSeriesData.Id, resultContext)
                .BindAsync(sm => sm.Where(m => m.Ids.AniDbSeriesId == aniDbSeriesId)
                    .Match(
                        () => resultContext.Failed(
                            $"No series mapping between TvDb series Id '{tvDbSeriesData.Id}' and AniDb series id '{aniDbSeriesId}'"),
                        Prelude.Right<ProcessFailedResult, ISeriesMapping>,
                        (head, tail) =>
                            resultContext.Failed(
                                $"Multiple series mappings found between TvDb series Id '{tvDbSeriesData.Id}' and AniDb series Id '{aniDbSeriesId}'")));

            return seriesMapping.BindAsync(sm =>
            {
                var episodeGroupMapping = sm.GetEpisodeGroupMapping(tvDbEpisodeData.AiredEpisodeNumber,
                    tvDbEpisodeData.AiredSeason);

                var aniDbEpisodeData = _episodeMapper.MapTvDbEpisodeAsync(tvDbEpisodeData.AiredEpisodeNumber,
                    sm, episodeGroupMapping);

                return aniDbEpisodeData.ToEither(resultContext.Failed(
                    $"Failed to find a corresponding AniDb episode in AniDb series id '{aniDbSeriesId}'"));
            });
        }

        private ISourceData CreateSourceData(AniDbEpisodeData episodeData, string title)
        {
            return new SourceData<AniDbEpisodeData>(_sources.AniDb, episodeData.Id,
                new ItemIdentifier(episodeData.EpisodeNumber.Number, episodeData.EpisodeNumber.SeasonNumber,
                    title), episodeData);
        }
    }
}