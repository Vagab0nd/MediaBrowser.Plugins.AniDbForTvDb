using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.SourceDataLoaders
{
    /// <summary>
    ///     Loads series data from TvDb based on the data provided by Emby
    /// </summary>
    internal class TvDbSeriesFromEmbyData : IEmbySourceDataLoader
    {
        private readonly ISources _sources;
        private readonly ITvDbClient _tvDbClient;

        public TvDbSeriesFromEmbyData(ITvDbClient tvDbClient, ISources sources)
        {
            _tvDbClient = tvDbClient;
            _sources = sources;
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

            return _tvDbClient.FindSeriesAsync(embyItemData.Identifier.Name)
                .ToEitherAsync(resultContext.Failed("Failed to find series in TvDb"))
                .MapAsync(s => CreateSourceData(s, embyItemData));
        }

        private ISourceData CreateSourceData(TvDbSeriesData seriesData, IEmbyItemData embyItemData)
        {
            return new SourceData<TvDbSeriesData>(_sources.TvDb, seriesData.Id,
                new ItemIdentifier(embyItemData.Identifier.Index, Option<int>.None, seriesData.SeriesName), seriesData);
        }
    }
}