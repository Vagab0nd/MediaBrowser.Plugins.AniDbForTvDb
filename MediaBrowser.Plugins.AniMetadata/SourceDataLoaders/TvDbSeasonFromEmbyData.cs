using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata.SourceDataLoaders
{
    /// <summary>
    ///     Loads season data from TvDb based on the data provided by Emby
    /// </summary>
    internal class TvDbSeasonFromEmbyData : IEmbySourceDataLoader
    {
        private readonly ISources sources;

        public TvDbSeasonFromEmbyData(ISources sources)
        {
            this.sources = sources;
        }

        public SourceName SourceName => SourceNames.TvDb;

        public bool CanLoadFrom(IMediaItemType mediaItemType)
        {
            return mediaItemType == MediaItemTypes.Season;
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IEmbyItemData embyItemData)
        {
            var seasonIdentifier = embyItemData.Identifier;

            return Right<ProcessFailedResult, ISourceData>(new IdentifierOnlySourceData(this.sources.TvDb, Option<int>.None,
                    seasonIdentifier))
                .AsTask();
        }
    }
}