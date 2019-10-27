namespace Emby.AniDbMetaStructure.JsonApi
{
    internal interface IRequest<TResponse>
    {
        string Url { get; }
    }
}