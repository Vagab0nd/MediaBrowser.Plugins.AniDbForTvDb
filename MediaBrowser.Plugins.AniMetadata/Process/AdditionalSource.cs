using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using MediaBrowser.Plugins.AniMetadata.SourceDataLoaders;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    /// <summary>
    ///     A source used as the source of additional data (data related to but not directly about the media item being
    ///     processed)
    ///     E.g. The series of an episode
    /// </summary>
    internal class AdditionalSource : ISource
    {
        private readonly ISource _sourceImplementation;

        public AdditionalSource(ISource sourceImplementation)
        {
            _sourceImplementation = sourceImplementation;
            Name = new SourceName(_sourceImplementation.Name + "_Additional");
        }

        public SourceName Name { get; }

        public Either<ProcessFailedResult, IEmbySourceDataLoader> GetEmbySourceDataLoader(IMediaItemType mediaItemType)
        {
            return _sourceImplementation.GetEmbySourceDataLoader(mediaItemType);
        }
    }
}