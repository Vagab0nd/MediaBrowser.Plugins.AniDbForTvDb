namespace MediaBrowser.Plugins.AniMetadata.TvDb.Requests
{
    internal class Response<TData>
    {
        public Response(TData data)
        {
            Data = data;
        }

        public TData Data { get; }
    }
}