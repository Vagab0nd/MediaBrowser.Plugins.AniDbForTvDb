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

        public async Task<Either<FailedRequest, Response<TResponseData>>> PostAsync<TResponseData>(IPostRequest<TResponseData> request,
            Option<string> token)
        {
            var requestOptions = new HttpRequestOptions
            {
                AcceptHeader = "application/json",
                Url = request.Url,
                RequestContent = _jsonSerialiser.Serialise(request.Data),
                RequestContentType = "application/json"
            };

            SetToken(requestOptions, token);

            _log.Debug($"Posting: '{requestOptions.RequestContent}' to '{requestOptions.Url}'");

            var response = await _httpClient.Post(requestOptions);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var content = new StreamReader(response.Content).ReadToEnd();

                _log.Debug($"Request failed: '{content}'");

                return new FailedRequest(response.StatusCode, content);
            }

            var responseDataString = GetStreamText(response.Content);

            _log.Debug($"Response: {responseDataString}");

            var responseData = _jsonSerialiser.Deserialise<TResponseData>(responseDataString);

            return new Response<TResponseData>(responseData);
        }

        public async Task<Either<FailedRequest, Response<TResponseData>>> GetAsync<TResponseData>(IGetRequest<TResponseData> request,
            Option<string> token)
        {
            var requestOptions = new HttpRequestOptions
            {
                AcceptHeader = "application/json",
                Url = request.Url
            };

            SetToken(requestOptions, token);

            _log.Debug($"Getting: '{requestOptions.Url}'");

            var response = await _httpClient.GetResponse(requestOptions);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var content = response?.Content == null ? null : new StreamReader(response.Content).ReadToEnd();

                return new FailedRequest(response.StatusCode, content);
            }

            var responseDataString = GetStreamText(response.Content);

            _log.Debug($"Response: {responseDataString}");

            var responseData = _jsonSerialiser.Deserialise<TResponseData>(responseDataString);

            return new Response<TResponseData>(responseData);
        }

        private void SetToken(HttpRequestOptions requestOptions, Option<string> token)
        {
            token.Iter(t => { requestOptions.RequestHeaders.Add("Authorization", $"Bearer {t}"); });
        }

        private string GetStreamText(Stream stream)
        {
            var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }
    }
}