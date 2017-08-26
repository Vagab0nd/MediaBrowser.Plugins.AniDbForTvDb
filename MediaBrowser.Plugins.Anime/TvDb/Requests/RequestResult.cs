using FunctionalSharp.DiscriminatedUnions;

namespace MediaBrowser.Plugins.Anime.TvDb.Requests
{
    internal class RequestResult<TData> : DiscriminatedUnion<Response<TData>, FailedRequest>
    {
        public RequestResult(object value) : base(value)
        {
        }

        public RequestResult(Response<TData> item) : base(item)
        {
        }

        public RequestResult(FailedRequest item) : base(item)
        {
        }

        protected RequestResult()
        {
        }
    }
}