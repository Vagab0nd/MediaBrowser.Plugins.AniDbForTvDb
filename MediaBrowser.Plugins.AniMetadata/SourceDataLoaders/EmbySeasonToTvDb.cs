using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata.SourceDataLoaders
{
    internal class EmbySeasonToTvDb : IEmbySourceDataLoader
    {
        private readonly ISources _sources;

        public EmbySeasonToTvDb(ISources sources)
        {
            _sources = sources;
        }

        public SourceName SourceName => SourceNames.TvDb;

        public bool CanLoadFrom(IMediaItemType mediaItemType)
        {
            return mediaItemType == MediaItemTypes.Season;
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IEmbyItemData embyItemData)
        {
            var seasonIdentifier = embyItemData.Identifier;

            return Right<ProcessFailedResult, ISourceData>(new IdentifierOnlySourceData(_sources.TvDb, Option<int>.None,
                    seasonIdentifier))
                .AsTask();
        }
    }
}