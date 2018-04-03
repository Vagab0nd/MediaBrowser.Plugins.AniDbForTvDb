namespace MediaBrowser.Plugins.AniMetadata.JsonApi
{
    internal interface IRequest<TResponse>
    {
        string Url { get; }
    }
}