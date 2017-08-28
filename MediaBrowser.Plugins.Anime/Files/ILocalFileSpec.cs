namespace MediaBrowser.Plugins.Anime.Files
{
    /// <summary>
    ///     A specification of a file which is populated by explicitly specifying the data
    /// </summary>
    /// <typeparam name="TRoot">The type of the serialised data</typeparam>
    internal interface ILocalFileSpec<out TRoot> : IFileSpec<TRoot> where TRoot : class
    {
    }
}