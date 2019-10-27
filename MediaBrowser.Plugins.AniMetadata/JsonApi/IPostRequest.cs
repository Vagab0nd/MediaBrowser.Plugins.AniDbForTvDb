namespace Emby.AniDbMetaStructure.JsonApi
{
    internal interface IPostRequest<TResponse> : IRequest<TResponse>
    {
        object Data { get; }
    }
}