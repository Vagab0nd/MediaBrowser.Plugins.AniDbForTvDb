using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Logging;

namespace MediaBrowser.Plugins.AniMetadata.JsonApi
{
    internal class JsonConnection : IJsonConnection
    {
        private readonly IHttpClient _httpClient;
        private readonly ICustomJsonSerialiser _jsonSerialiser;
        private readonly ILogger _log;

        public JsonConnection(IHttpClient httpClient, ICustomJsonSerialiser jsonSerialiser, ILogManager logManager)
        {
            _httpClient = httpClient;
            _jsonSerialiser = jsonSerialiser;
            _log = logManager.GetLogger(nameof(JsonConnection));
        }

        public Task<Either<FailedRequest, Response<TResponseData>>> PostAsync<TResponseData>(
            IPostRequest<TResponseData> request, Option<string> oAuthAccessToken)
        {
            return PostAsync(request, oAuthAccessToken, ParseResponse<TResponseData>);
        }

        public Task<Either<FailedRequest, Response<TResponseData>>> GetAsync<TResponseData>(
            IGetRequest<TResponseData> request, Option<string> oAuthAccessToken)
        {
            return GetAsync(request, oAuthAccessToken, ParseResponse<TResponseData>);
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
                RequestContent = new ReadOnlyMemory<char>(_jsonSerialiser.Serialise(request.Data).ToCharArray()),
                RequestContentType = "application/json"
            };

            SetToken(requestOptions, oAuthAccessToken);

            _log.Debug($"Posting: '{requestOptions.RequestContent}' to '{requestOptions.Url}'");

            var response = _httpClient.Post(requestOptions);

            return response.Map(r => ApplyResponseHandler(responseHandler, r));
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

            SetToken(requestOptions, oAuthAccessToken);

            _log.Debug($"Getting: '{requestOptions.Url}'");

            var response = _httpClient.GetResponse(requestOptions);

            return response.Map(r => ApplyResponseHandler(responseHandler, r));
        }

        private Either<TFailedRequest, Response<TResponseData>> ApplyResponseHandler<TFailedRequest, TResponseData>(
            Func<string, ICustomJsonSerialiser, HttpResponseInfo, Either<TFailedRequest, Response<TResponseData>>>
                responseHandler, HttpResponseInfo response)
        {
            var responseContent = GetStreamText(response.Content);

            _log.Debug(response.StatusCode != HttpStatusCode.OK
                ? $"Request failed (http {response.StatusCode}): '{responseContent}'"
                : $"Response: {responseContent}");

            return responseHandler(responseContent, _jsonSerialiser, response);
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