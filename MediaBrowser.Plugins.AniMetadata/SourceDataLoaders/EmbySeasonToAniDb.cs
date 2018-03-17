using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.Process;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata.SourceDataLoaders
{
    internal class EmbySeasonToAniDb : IEmbySourceDataLoader
    {
        private readonly ISources _sources;

        public EmbySeasonToAniDb(ISources sources)
        {
            _sources = sources;
        }

        public SourceName SourceName => SourceNames.AniDb;

        public bool CanLoadFrom(IMediaItemType mediaItemType)
        {
            return mediaItemType == MediaItemTypes.Season;
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IEmbyItemData embyItemData)
        {
            var seasonIdentifier = new ItemIdentifier(embyItemData.Identifier.Index.IfNone(1),
                embyItemData.Identifier.ParentIndex, embyItemData.Identifier.Name);

            return Right<ProcessFailedResult, ISourceData>(
                    new IdentifierOnlySourceData(_sources.AniDb, Option<int>.None, seasonIdentifier))
                .AsTask();
        }
    }
}