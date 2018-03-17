using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Mapping;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.SourceDataLoaders
{
    internal class AniDbSeriesToTvDb : ISourceDataLoader
    {
        private readonly IMappingList _mappingList;
        private readonly ISources _sources;

        public AniDbSeriesToTvDb(ISources sources, IMappingList mappingList)
        {
            _sources = sources;
            _mappingList = mappingList;
        }

        public bool CanLoadFrom(object sourceData)
        {
            return sourceData is ISourceData<AniDbSeriesData>;
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IMediaItem mediaItem, object sourceData)
        {
            var aniDbSourceData = (ISourceData<AniDbSeriesData>)sourceData;

            var resultContext = new ProcessResultContext(nameof(AniDbSeriesToTvDb), mediaItem.EmbyData.Identifier.Name,
                mediaItem.ItemType);

            return aniDbSourceData.Id.ToEither(
                    resultContext.Failed(
                        "No AniDb Id found on the AniDb data associated with this media item"))
                .BindAsync(aniDbSeriesId => GetMappedTvDbSeriesId(aniDbSeriesId, resultContext))
                .BindAsync(tvDbSeriesId => _sources.TvDb.GetSeriesData(tvDbSeriesId, resultContext))
                .MapAsync(CreateSourceData);
        }

        private Task<Either<ProcessFailedResult, int>> GetMappedTvDbSeriesId(int aniDbSeriesId,
            ProcessResultContext resultContext)
        {
            return _mappingList.GetSeriesMappingFromAniDb(aniDbSeriesId, resultContext)
                .BindAsync(sm =>
                    sm.Ids.TvDbSeriesId.ToEither(resultContext.Failed("No TvDb Id found on matching mapping")));
        }

        private ISourceData CreateSourceData(TvDbSeriesData tvDbSeriesData)
        {
            return new SourceData<TvDbSeriesData>(_sources.TvDb,
                tvDbSeriesData.Id,
                new ItemIdentifier(Option<int>.None, Option<int>.None, tvDbSeriesData.SeriesName),
                tvDbSeriesData);
        }
    }
}