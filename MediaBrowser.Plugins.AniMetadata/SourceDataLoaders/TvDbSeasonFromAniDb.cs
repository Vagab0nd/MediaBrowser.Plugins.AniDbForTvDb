using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.Process;

namespace MediaBrowser.Plugins.AniMetadata.SourceDataLoaders
{
    /// <summary>
    ///     Loads TvDb season data based on existing AniDb series information
    /// </summary>
    internal class TvDbSeasonFromAniDb : ISourceDataLoader
    {
        private readonly ISources _sources;

        public TvDbSeasonFromAniDb(ISources sources)
        {
            _sources = sources;
        }

        public bool CanLoadFrom(object sourceData)
        {
            return true;
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IMediaItem mediaItem, object sourceData)
        {
            var resultContext = new ProcessResultContext(nameof(TvDbSeasonFromAniDb),
                mediaItem.EmbyData.Identifier.Name,
                mediaItem.ItemType);

            return mediaItem.EmbyData.Identifier.Index
                .ToEither(resultContext.Failed("No season index provided by Emby"))
                .Map(CreateSourceData)
                .AsTask();
        }

        private ISourceData CreateSourceData(int seasonIndex)
        {
            return new IdentifierOnlySourceData(_sources.TvDb, seasonIndex,
                new ItemIdentifier(seasonIndex, Option<int>.None, $"Season {seasonIndex}"));
        }
    }
}