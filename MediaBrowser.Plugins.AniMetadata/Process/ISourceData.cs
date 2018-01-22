namespace MediaBrowser.Plugins.AniMetadata.Process
{
    /// <summary>
    ///     The metadata for an item from a single source
    /// </summary>
    internal interface ISourceData
    {
        /// <summary>
        ///     The source of the metadata
        /// </summary>
        ISource Source { get; }

        /// <summary>
        ///     The identity of the media item according to this source,
        ///     and which was used to retrieve this metadata
        /// </summary>
        IItemIdentifier Identifier { get; }

        /// <summary>
        ///     Get the metadata about the item from this source
        /// </summary>
        /// <typeparam name="TData">The type of data returned by this source</typeparam>
        TData GetData<TData>() where TData : class;
    }
}