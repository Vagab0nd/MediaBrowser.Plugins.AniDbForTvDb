using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Mapping;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;
using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata.SourceDataLoaders
{
    /// <summary>
    ///     Loads TvDb episode data based on data from AniDb
    /// </summary>
    internal class AniDbEpisodeToTvDb : ISourceDataLoader
    {
        private readonly IEpisodeMapper _episodeMapper;
        private readonly IMappingList _mappingList;
        private readonly ISources _sources;

        public AniDbEpisodeToTvDb(ISources sources, IMappingList mappingList, IEpisodeMapper episodeMapper)
        {
            _sources = sources;
            _mappingList = mappingList;
            _episodeMapper = episodeMapper;
        }

        public bool CanLoadFrom(object sourceData)
        {
            return sourceData is ISourceData<AniDbEpisodeData>;
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IMediaItem mediaItem, object sourceData)
        {
            var aniDbEpisodeData = ((ISourceData<AniDbEpisodeData>)sourceData).Data;

            var resultContext = new ProcessResultContext(nameof(AniDbEpisodeToTvDb), mediaItem.EmbyData.Identifier.Name,
                mediaItem.ItemType);

            var aniDbSeriesData = _sources.AniDb.GetSeriesData(mediaItem.EmbyData, resultContext);

            var tvDbEpisodeData =
                aniDbSeriesData.BindAsync(
                    seriesData => MapEpisodeDataAsync(seriesData, aniDbEpisodeData, resultContext));

            return tvDbEpisodeData.MapAsync(episodeData => (ISourceData)new SourceData<TvDbEpisodeData>(_sources.TvDb,
                episodeData.Id,
                new ItemIdentifier(episodeData.AiredEpisodeNumber, episodeData.AiredSeason,
                    episodeData.EpisodeName), episodeData));
        }

        private Task<Either<ProcessFailedResult, TvDbEpisodeData>> MapEpisodeDataAsync(AniDbSeriesData aniDbSeriesData,
            AniDbEpisodeData aniDbEpisodeData, ProcessResultContext resultContext)
        {
            var seriesMapping = _mappingList.GetSeriesMappingFromAniDb(aniDbSeriesData.Id, resultContext);

            return seriesMapping.BindAsync(sm =>
                {
                    var episodeGroupMapping = sm.GetEpisodeGroupMapping(aniDbEpisodeData.EpisodeNumber);

                    var tvDbEpisodeData = _episodeMapper.MapAniDbEpisodeAsync(aniDbEpisodeData.EpisodeNumber.Number,
                        sm, episodeGroupMapping);

                    return tvDbEpisodeData.Match(
                        d => Right<ProcessFailedResult, TvDbEpisodeData>(d),
                        () => resultContext.Failed("Found a series mapping but failed to map the episode to TvDb"));
                });
        }
    }
}