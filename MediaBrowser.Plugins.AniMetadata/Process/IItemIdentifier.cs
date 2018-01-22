namespace MediaBrowser.Plugins.AniMetadata.Process
{
    /// <summary>
    ///     The identity of a <see cref="IMediaItem" />
    /// </summary>
    internal interface IItemIdentifier
    {
        int? Index { get; }

        int? ParentIndex { get; }

        string Name { get; }
    }
}