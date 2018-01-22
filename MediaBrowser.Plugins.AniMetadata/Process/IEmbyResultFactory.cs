namespace MediaBrowser.Plugins.AniMetadata.Process
{
    /// <summary>
    ///     Produces Emby results from <see cref="IMediaItem" />s
    /// </summary>
    internal interface IEmbyResultFactory
    {
        IEmbyResult GetResult(IMediaItem mediaItem);
    }
}