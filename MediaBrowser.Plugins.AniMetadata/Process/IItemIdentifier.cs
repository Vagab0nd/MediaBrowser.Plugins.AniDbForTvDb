using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    /// <summary>
    ///     The identity of a <see cref="IMediaItem" />
    /// </summary>
    internal interface IItemIdentifier
    {
        Option<int> Index { get; }

        Option<int> ParentIndex { get; }

        string Name { get; }
    }
}