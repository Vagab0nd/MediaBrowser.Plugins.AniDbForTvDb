namespace MediaBrowser.Plugins.AniMetadata.JsonApi
{
    internal interface IPostRequest<TResponse> : IRequest<TResponse>
    {
        object Data { get; }
    }
}