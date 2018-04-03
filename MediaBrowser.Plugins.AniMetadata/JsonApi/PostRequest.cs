namespace MediaBrowser.Plugins.AniMetadata.JsonApi
{
    internal abstract class PostRequest<TResponse> : Request<TResponse>, IPostRequest<TResponse>
    {
        protected PostRequest(string url, object data) : base(url)
        {
            Data = data;
        }

        public object Data { get; }
    }
}