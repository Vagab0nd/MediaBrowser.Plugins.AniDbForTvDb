using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using MediaBrowser.Plugins.AniMetadata.Providers.AniDb;

namespace MediaBrowser.Plugins.AniMetadata.SourceDataLoaders
{
    /// <summary>
    ///     Loads episode data from AniDb based on the data provided by Emby
    /// </summary>
    internal class AniDbEpisodeFromEmbyData : IEmbySourceDataLoader
    {
        private readonly IAniDbEpisodeMatcher aniDbEpisodeMatcher;
        private readonly ISources sources;

        public AniDbEpisodeFromEmbyData(ISources sources, IAniDbEpisodeMatcher aniDbEpisodeMatcher)
        {
            this.sources = sources;
            this.aniDbEpisodeMatcher = aniDbEpisodeMatcher;
        }

        public SourceName SourceName => SourceNames.AniDb;

        public bool CanLoadFrom(IMediaItemType mediaItemType)
        {
            return mediaItemType == MediaItemTypes.Episode;
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IEmbyItemData embyItemData)
        {
            var resultContext = new ProcessResultContext(nameof(AniDbEpisodeFromEmbyData), embyItemData.Identifier.Name,
                embyItemData.ItemType);

            return this.sources.AniDb.GetSeriesData(embyItemData, resultContext)
                .BindAsync(seriesData => GetAniDbEpisodeData(seriesData, embyItemData, resultContext))
                .BindAsync(episodeData =>
                {
                    var title = this.sources.AniDb.SelectTitle(episodeData.Titles, embyItemData.Language, resultContext);

                    return title.Map(t => CreateSourceData(episodeData, t));
                });
        }

        private Either<ProcessFailedResult, AniDbEpisodeData> GetAniDbEpisodeData(AniDbSeriesData aniDbSeriesData,
            IEmbyItemData embyItemData,
            ProcessResultContext resultContext)
        {
            return this.aniDbEpisodeMatcher.FindEpisode(aniDbSeriesData.Episodes,
                    embyItemData.Identifier.ParentIndex,
                    embyItemData.Identifier.Index, embyItemData.Identifier.Name)
                .ToEither(resultContext.Failed("Failed to find episode in AniDb"));
        }

        private ISourceData CreateSourceData(AniDbEpisodeData e, string title)
        {
            return new SourceData<AniDbEpisodeData>(this.sources.AniDb, e.Id,
                new ItemIdentifier(e.EpisodeNumber.Number, e.EpisodeNumber.SeasonNumber, title), e);
        }
    }
}