using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Emby.AniDbMetaStructure.Infrastructure;
using LanguageExt;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Logging;

namespace Emby.AniDbMetaStructure.JsonApi
{
    internal class JsonConnection : IJsonConnection
    {
        private readonly IHttpClient httpClient;
        private readonly ICustomJsonSerialiser jsonSerialiser;
        private readonly ILogger log;

        public JsonConnection(IHttpClient httpClient, ICustomJsonSerialiser jsonSerialiser, ILogManager logManager)
        {
            this.httpClient = httpClient;
            this.jsonSerialiser = jsonSerialiser;
            this.log = logManager.GetLogger(nameof(JsonConnection));
        }

        public Task<Either<FailedRequest, Response<TResponseData>>> PostAsync<TResponseData>(
            IPostRequest<TResponseData> request, Option<string> oAuthAccessToken)
        {
            return this.PostAsync(request, oAuthAccessToken, this.ParseResponse<TResponseData>);
        }

        public Task<Either<FailedRequest, Response<TResponseData>>> GetAsync<TResponseData>(
            IGetRequest<TResponseData> request, Option<string> oAuthAccessToken)
        {
            return this.GetAsync(request, oAuthAccessToken, this.ParseResponse<TResponseData>);
        }

        public Task<Either<TFailedRequest, Response<TResponseData>>> PostAsync<TFailedRequest, TResponseData>(
            IPostRequest<TResponseData> request, Option<string> oAuthAccessToken,
            Func<string, ICustomJsonSerialiser, HttpResponseInfo, Either<TFailedRequest, Response<TResponseData>>>
                responseHandler)
        {
            var requestOptions = new HttpRequestOptions
            {
                AcceptHeader = "application/json",
                Url = request.Url,
                RequestContent = new ReadOnlyMemory<char>(this.jsonSerialiser.Serialise(request.Data).ToCharArray()),
                RequestContentType = "application/json"
            };

            this.SetToken(requestOptions, oAuthAccessToken);

            this.log.Debug($"Posting: '{requestOptions.RequestContent}' to '{requestOptions.Url}'");

            var response = this.httpClient.Post(requestOptions);

            return response.Map(r => this.ApplyResponseHandler(responseHandler, r));
        }

        public Task<Either<TFailedRequest, Response<TResponseData>>> GetAsync<TFailedRequest, TResponseData>(
            IGetRequest<TResponseData> request, Option<string> oAuthAccessToken,
            Func<string, ICustomJsonSerialiser, HttpResponseInfo, Either<TFailedRequest, Response<TResponseData>>>
                responseHandler)
        {
            var requestOptions = new HttpRequestOptions
            {
                AcceptHeader = "application/json",
                Url = request.Url
            };

            this.SetToken(requestOptions, oAuthAccessToken);

            this.log.Debug($"Getting: '{requestOptions.Url}'");

            var response = this.httpClient.GetResponse(requestOptions);

            return response.Map(r => this.ApplyResponseHandler(responseHandler, r));
        }

        private Either<TFailedRequest, Response<TResponseData>> ApplyResponseHandler<TFailedRequest, TResponseData>(
            Func<string, ICustomJsonSerialiser, HttpResponseInfo, Either<TFailedRequest, Response<TResponseData>>>
                responseHandler, HttpResponseInfo response)
        {
            var responseContent = this.GetStreamText(response.Content);

            this.log.Debug(response.StatusCode != HttpStatusCode.OK
                ? $"Request failed (http {response.StatusCode}): '{responseContent}'"
                : $"Response: {responseContent}");

            return responseHandler(responseContent, this.jsonSerialiser, response);
        }

        private Either<FailedRequest, Response<TResponseData>> ParseResponse<TResponseData>(string responseContent,
            ICustomJsonSerialiser serialiser, HttpResponseInfo response)
        {
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new FailedRequest(response.StatusCode, responseContent);
            }

            return new Response<TResponseData>(serialiser.Deserialise<TResponseData>(responseContent));
        }

        private void SetToken(HttpRequestOptions requestOptions, Option<string> token)
        {
            token.IfSome(t => requestOptions.RequestHeaders.Add("Authorization", $"Bearer {t}"));
        }

        private string GetStreamText(Stream stream)
        {
            if (stream == null)
            {
                return null;
            }

            var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }
    }
}