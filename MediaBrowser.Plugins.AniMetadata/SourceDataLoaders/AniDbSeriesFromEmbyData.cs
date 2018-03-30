using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;

namespace MediaBrowser.Plugins.AniMetadata.SourceDataLoaders
{
    /// <summary>
    ///     Loads series data from AniDb based on the data provided by Emby
    /// </summary>
    internal class AniDbSeriesFromEmbyData : IEmbySourceDataLoader
    {
        private readonly IAniDbClient _aniDbClient;
        private readonly ISources _sources;

        public AniDbSeriesFromEmbyData(IAniDbClient aniDbClient, ISources sources)
        {
            _aniDbClient = aniDbClient;
            _sources = sources;
        }

        public SourceName SourceName => SourceNames.AniDb;

        public bool CanLoadFrom(IMediaItemType mediaItemType)
        {
            return mediaItemType == MediaItemTypes.Series;
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IEmbyItemData embyItemData)
        {
            var resultContext = new ProcessResultContext(nameof(AniDbSeriesFromEmbyData), embyItemData.Identifier.Name,
                embyItemData.ItemType);

            return _aniDbClient.FindSeriesAsync(embyItemData.Identifier.Name)
                .ToEitherAsync(resultContext.Failed("Failed to find series in AniDb"))
                .BindAsync(s =>
                {
                    var title = _sources.AniDb.SelectTitle(s.Titles, embyItemData.Language, resultContext);

                    return title.Map(t => CreateSourceData(s, embyItemData, t));
                });
        }

        private ISourceData CreateSourceData(AniDbSeriesData seriesData, IEmbyItemData embyItemData, string title)
        {
            return new SourceData<AniDbSeriesData>(_sources.AniDb, seriesData.Id,
                new ItemIdentifier(embyItemData.Identifier.Index, Option<int>.None, title), seriesData);
        }
    }
}