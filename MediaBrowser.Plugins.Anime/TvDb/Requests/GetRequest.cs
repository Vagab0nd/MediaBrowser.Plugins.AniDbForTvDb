namespace MediaBrowser.Plugins.AniMetadata.TvDb.Requests
{
    internal abstract class GetRequest<TResponse> : Request<TResponse>
    {
        public GetRequest(string urlPath) : base(urlPath)
        {
        }
    }
}