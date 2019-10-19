namespace MediaBrowser.Plugins.AniMetadata.SourceDataLoaders
{
    using System.Threading.Tasks;
    using AniDb.SeriesData;
    using LanguageExt;
    using Process;
    using TvDb.Data;

    internal class TvDbSeriesFromTvDb : ISourceDataLoader
    {
        private readonly ISources sources;

        public TvDbSeriesFromTvDb(ISources sources)
        {
            this.sources = sources;
        }

        public bool CanLoadFrom(object sourceData)
        {
            return sourceData is ISourceData<TvDbSeriesData>;
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IMediaItem mediaItem, object sourceData)
        {
            var tvDbSourceData = (ISourceData<TvDbSeriesData>)sourceData;

            var resultContext = new ProcessResultContext(nameof(TvDbSeriesFromTvDb),
                mediaItem.EmbyData.Identifier.Name,
                mediaItem.ItemType);

            mediaItem.AddData(this.CreateAdditionalSourceData()).IfRight(mi => mediaItem = mi);

            return tvDbSourceData.Id.ToEither(
                    resultContext.Failed(
                        "No TvDB Id found on the TvDB data associated with this media item"))
                .BindAsync(tvDbSeriesId => this.sources.TvDb.GetSeriesData(tvDbSeriesId, resultContext))
                //.BindAsync(tvDbSeriesData => this.AddEmptyAniDbSourceData(tvDbSeriesData, mediaItem))
                .MapAsync(CreateSourceData);
        }

        private ISourceData CreateSourceData(TvDbSeriesData tvDbSeriesData)
        {
            return new SourceData<TvDbSeriesData>(this.sources.TvDb,
                tvDbSeriesData.Id,
                new ItemIdentifier(Option<int>.None, Option<int>.None, tvDbSeriesData.SeriesName),
                tvDbSeriesData);
        }

        public Task<Either<ProcessFailedResult, TvDbSeriesData>> AddEmptyAniDbSourceData(TvDbSeriesData tvDbSeriesData, IMediaItem mediaItem)
        {
            //mediaItem.AddData(this.CreateAdditionalSourceData(tvDbSeriesData));

            return new Task<Either<ProcessFailedResult, TvDbSeriesData>>(() => tvDbSeriesData);
        }

        private ISourceData CreateAdditionalSourceData(
            //TvDbSeriesData tvDbSeriesData
            )
        {
            return new SourceData<AniDbSeriesData>(this.sources.AniDb,
                Option<int>.None,
                new ItemIdentifier(Option<int>.None, Option<int>.None, string.Empty),
                new AniDbSeriesData());
        }
    }
}
