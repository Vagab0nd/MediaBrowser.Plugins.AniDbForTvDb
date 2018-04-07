namespace MediaBrowser.Plugins.AniMetadata.JsonApi
{
    public class Response<TResponseData>
    {
        public Response(TResponseData data)
        {
            Data = data;
        }

        public TResponseData Data { get; }
    }
}