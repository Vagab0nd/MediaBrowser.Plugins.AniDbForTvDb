namespace MediaBrowser.Plugins.AniMetadata.JsonApi
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