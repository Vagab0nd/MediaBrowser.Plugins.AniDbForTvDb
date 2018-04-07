using System;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Common.Net;

namespace MediaBrowser.Plugins.AniMetadata.JsonApi
{
    internal interface IJsonConnection
    {
        Task<Either<FailedRequest, Response<TResponseData>>> PostAsync<TResponseData>(
            IPostRequest<TResponseData> request, Option<string> oAuthAccessToken);

        Task<Either<TFailedRequest, Response<TResponseData>>> PostAsync<TFailedRequest, TResponseData>(
            IPostRequest<TResponseData> request, Option<string> oAuthAccessToken,
            Func<string, ICustomJsonSerialiser, HttpResponseInfo, Either<TFailedRequest, Response<TResponseData>>>
                responseHandler);

        Task<Either<FailedRequest, Response<TResponseData>>> GetAsync<TResponseData>(
            IGetRequest<TResponseData> request, Option<string> oAuthAccessToken);

        Task<Either<TFailedRequest, Response<TResponseData>>> GetAsync<TFailedRequest, TResponseData>(
            IGetRequest<TResponseData> request, Option<string> oAuthAccessToken,
            Func<string, ICustomJsonSerialiser, HttpResponseInfo, Either<TFailedRequest, Response<TResponseData>>>
                responseHandler);
    }
}