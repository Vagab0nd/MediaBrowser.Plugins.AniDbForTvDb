using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.SourceDataLoaders;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.Process.Sources
{
    internal class TvDbSource : ITvDbSource
    {
        private readonly IEnumerable<IEmbySourceDataLoader> _embySourceDataLoaders;
        private readonly ITvDbClient _tvDbClient;

        public TvDbSource(ITvDbClient tvDbClient, IEnumerable<IEmbySourceDataLoader> embySourceDataLoaders)
        {
            _tvDbClient = tvDbClient;
            _embySourceDataLoaders = embySourceDataLoaders;
        }

        public SourceName Name => SourceNames.TvDb;

        public Either<ProcessFailedResult, IEmbySourceDataLoader> GetEmbySourceDataLoader(IMediaItemType mediaItemType)
        {
            return _embySourceDataLoaders.Find(l => l.SourceName == Name && l.CanLoadFrom(mediaItemType))
                .ToEither(new ProcessFailedResult(Name, "", mediaItemType,
                    "No Emby source data loader for this source and media item type"));
        }

        public Task<Either<ProcessFailedResult, TvDbSeriesData>> GetSeriesData(IEmbyItemData embyItemData,
            ProcessResultContext resultContext)
        {
            Task<Either<ProcessFailedResult, int>> seriesId;

            if (embyItemData.ItemType == MediaItemTypes.Series)
            {
                seriesId = embyItemData.GetExistingId(Name)
                    .ToEitherAsync(resultContext.Failed("No TvDb Id found on this series"));
            }
            else
            {
                seriesId = embyItemData.GetParentId(MediaItemTypes.Series, this)
                    .ToEitherAsync(resultContext.Failed("No TvDb Id found on parent series"));
            }

            return seriesId.BindAsync(tvDbSeriesId => GetSeriesData(tvDbSeriesId, resultContext));
        }

        public Task<Either<ProcessFailedResult, TvDbSeriesData>> GetSeriesData(int tvDbSeriesId,
            ProcessResultContext resultContext)
        {
            return _tvDbClient.GetSeriesAsync(tvDbSeriesId)
                .ToEitherAsync(resultContext.Failed($"Failed to load parent series with TvDb Id '{tvDbSeriesId}'"));
        }
    }
}