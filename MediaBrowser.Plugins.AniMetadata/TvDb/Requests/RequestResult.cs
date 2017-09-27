using OneOf;

namespace MediaBrowser.Plugins.AniMetadata.TvDb.Requests
{
    internal class RequestResult<TData> : OneOfBase<Response<TData>, FailedRequest>
    {
        protected RequestResult(int index, Response<TData> value0 = null, FailedRequest value1 = null) : base(index,
            value0, value1)
        {
        }

        public static implicit operator RequestResult<TData>(Response<TData> value)
        {
            return new RequestResult<TData>(0, value);
        }

        public static implicit operator RequestResult<TData>(FailedRequest value)
        {
            return new RequestResult<TData>(1, null, value);
        }
    }
}