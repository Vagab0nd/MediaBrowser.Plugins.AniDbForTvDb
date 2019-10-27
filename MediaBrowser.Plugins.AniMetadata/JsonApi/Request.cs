namespace Emby.AniDbMetaStructure.JsonApi
{
    internal abstract class Request<TResponse> : IRequest<TResponse>
    {
        protected Request(string url)
        {
            this.Url = url;
        }

        public string Url { get; }
    }
}