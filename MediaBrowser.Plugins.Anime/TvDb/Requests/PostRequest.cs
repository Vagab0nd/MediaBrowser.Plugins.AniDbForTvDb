namespace MediaBrowser.Plugins.Anime.TvDb.Requests
{
    internal abstract class PostRequest<TResponse> : Request<TResponse>
    {
        public PostRequest(string urlPath, object data) : base(urlPath)
        {
            Data = data;
        }

        public object Data { get; }
    }
}