namespace MediaBrowser.Plugins.Anime.AniDb
{
    /// <summary>
    ///     A specification of a file containing serialised data of a known type
    /// </summary>
    /// <typeparam name="TRoot">The type of the serialised data</typeparam>
    internal interface IFileSpec<out TRoot> where TRoot : class
    {
        /// <summary>
        ///     The local path to the file
        /// </summary>
        string LocalPath { get; }
    }

    /// <summary>
    ///     A specification of a file which is populated by explicitly specifying the data
    /// </summary>
    /// <typeparam name="TRoot">The type of the serialised data</typeparam>
    internal interface ILocalFileSpec<out TRoot> : IFileSpec<TRoot> where TRoot : class
    {
    }

    /// <summary>
    ///     A specification of a remote file which can be downloaded on demand
    /// </summary>
    /// <typeparam name="TRoot">The type of the serialised data</typeparam>
    internal interface IRemoteFileSpec<out TRoot> : IFileSpec<TRoot> where TRoot : class
    {
        bool IsGZipped { get; }

        string Url { get; }
    }
}