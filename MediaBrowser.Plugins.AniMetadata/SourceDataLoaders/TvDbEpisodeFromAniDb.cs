using System.Threading.Tasks;
using Emby.AniDbMetaStructure.AniDb.SeriesData;
using Emby.AniDbMetaStructure.Mapping;
using Emby.AniDbMetaStructure.Process;
using Emby.AniDbMetaStructure.TvDb.Data;
using LanguageExt;
using static LanguageExt.Prelude;

namespace Emby.AniDbMetaStructure.SourceDataLoaders
{
    /// <summary>
    ///     Loads TvDb episode data based on data from AniDb
    /// </summary>
    internal class TvDbEpisodeFromAniDb : ISourceDataLoader
    {
        private readonly IEpisodeMapper episodeMapper;
        private readonly IMappingList mappingList;
        private readonly ISources sources;

        public TvDbEpisodeFromAniDb(ISources sources, IMappingList mappingList, IEpisodeMapper episodeMapper)
        {
            this.sources = sources;
            this.mappingList = mappingList;
            this.episodeMapper = episodeMapper;
        }

        public bool CanLoadFrom(object sourceData)
        {
            return sourceData is ISourceData<AniDbEpisodeData>;
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IMediaItem mediaItem, object sourceData)
        {
            var aniDbEpisodeData = ((ISourceData<AniDbEpisodeData>)sourceData).Data;

            var resultContext = new ProcessResultContext(nameof(TvDbEpisodeFromAniDb), mediaItem.EmbyData.Identifier.Name,
                mediaItem.ItemType);

            var aniDbSeriesData = this.sources.AniDb.GetSeriesData(mediaItem.EmbyData, resultContext);

            var tvDbEpisodeData =
                aniDbSeriesData.BindAsync(
                    seriesData => this.MapEpisodeDataAsync(seriesData, aniDbEpisodeData, resultContext));

            return tvDbEpisodeData.MapAsync(episodeData => (ISourceData)new SourceData<TvDbEpisodeData>(this.sources.TvDb,
                episodeData.Id,
                new ItemIdentifier(episodeData.AiredEpisodeNumber, episodeData.AiredSeason,
                    episodeData.EpisodeName), episodeData));
        }

        private Task<Either<ProcessFailedResult, TvDbEpisodeData>> MapEpisodeDataAsync(AniDbSeriesData aniDbSeriesData,
            AniDbEpisodeData aniDbEpisodeData, ProcessResultContext resultContext)
        {
            var seriesMapping = this.mappingList.GetSeriesMappingFromAniDb(aniDbSeriesData.Id, resultContext);

            return seriesMapping.BindAsync(sm =>
                {
                    var episodeGroupMapping = sm.GetEpisodeGroupMapping(aniDbEpisodeData.EpisodeNumber);

                    var tvDbEpisodeData = this.episodeMapper.MapAniDbEpisodeAsync(aniDbEpisodeData.EpisodeNumber.Number,
                        sm, episodeGroupMapping);

                    return tvDbEpisodeData.Match(
                        d => Right<ProcessFailedResult, TvDbEpisodeData>(d),
                        () => resultContext.Failed("Found a series mapping but failed to map the episode to TvDb"));
                });
        }
    }
}