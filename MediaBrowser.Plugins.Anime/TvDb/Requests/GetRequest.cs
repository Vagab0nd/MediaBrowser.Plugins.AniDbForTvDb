namespace MediaBrowser.Plugins.Anime.TvDb.Requests
{
    internal abstract class GetRequest<TResponse> : Request<TResponse>
    {
        public GetRequest(string urlPath) : base(urlPath)
        {
        }
    }
}