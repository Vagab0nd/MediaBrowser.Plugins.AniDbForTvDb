using System.Threading.Tasks;
using Emby.AniDbMetaStructure.Process;
using Emby.AniDbMetaStructure.Process.Sources;
using Emby.AniDbMetaStructure.TvDb;
using Emby.AniDbMetaStructure.TvDb.Data;
using LanguageExt;

namespace Emby.AniDbMetaStructure.SourceDataLoaders
{
    /// <summary>
    ///     Loads series data from TvDb based on the data provided by Emby
    /// </summary>
    internal class TvDbSeriesFromEmbyData : IEmbySourceDataLoader
    {
        private readonly ISources sources;
        private readonly ITvDbClient tvDbClient;

        public TvDbSeriesFromEmbyData(ITvDbClient tvDbClient, ISources sources)
        {
            this.tvDbClient = tvDbClient;
            this.sources = sources;
        }

        public SourceName SourceName => SourceNames.TvDb;

        public bool CanLoadFrom(IMediaItemType mediaItemType)
        {
            return mediaItemType == MediaItemTypes.Series;
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IEmbyItemData embyItemData)
        {
            var resultContext = new ProcessResultContext(nameof(TvDbSeriesFromEmbyData), embyItemData.Identifier.Name,
                embyItemData.ItemType);

            return this.tvDbClient.FindSeriesAsync(embyItemData.Identifier.Name)
                .ToEitherAsync(resultContext.Failed("Failed to find series in TvDb"))
                .MapAsync(s => this.CreateSourceData(s, embyItemData));
        }

        private ISourceData CreateSourceData(TvDbSeriesData seriesData, IEmbyItemData embyItemData)
        {
            return new SourceData<TvDbSeriesData>(this.sources.TvDb, seriesData.Id,
                new ItemIdentifier(embyItemData.Identifier.Index, Option<int>.None, seriesData.SeriesName), seriesData);
        }
    }
}