namespace MediaBrowser.Plugins.Anime.TvDb.Requests
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