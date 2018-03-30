using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.SourceDataLoaders
{
    /// <summary>
    ///     Loads series data for an item that already has TvDb episode data loaded
    /// </summary>
    internal class TvDbSeriesFromTvDbEpisode : ISourceDataLoader
    {
        private readonly ISources _sources;

        public TvDbSeriesFromTvDbEpisode(ISources sources)
        {
            _sources = sources;
        }

        public bool CanLoadFrom(object sourceData)
        {
            return sourceData is ISourceData<TvDbEpisodeData>;
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IMediaItem mediaItem, object sourceData)
        {
            var resultContext = new ProcessResultContext(nameof(TvDbSeriesFromEmbyData),
                mediaItem.EmbyData.Identifier.Name,
                mediaItem.EmbyData.ItemType);

            return _sources.TvDb.GetSeriesData(mediaItem.EmbyData, resultContext)
                .MapAsync(s => CreateSourceData(s, mediaItem.EmbyData));
        }

        private ISourceData CreateSourceData(TvDbSeriesData seriesData, IEmbyItemData embyItemData)
        {
            return new SourceData<TvDbSeriesData>(_sources.TvDb.ForAdditionalData(), seriesData.Id,
                new ItemIdentifier(embyItemData.Identifier.Index, Option<int>.None, seriesData.SeriesName), seriesData);
        }
    }
}