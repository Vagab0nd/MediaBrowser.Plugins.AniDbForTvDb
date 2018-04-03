namespace MediaBrowser.Plugins.AniMetadata.JsonApi
{
    internal abstract class Request<TResponse> : IRequest<TResponse>
    {
        protected Request(string url)
        {
            Url = url;
        }

        public string Url { get; }
    }
}