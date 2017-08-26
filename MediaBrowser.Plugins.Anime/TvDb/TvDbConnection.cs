using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Plugins.Anime.TvDb.Requests;

namespace MediaBrowser.Plugins.Anime.TvDb
{
    internal class TvDbConnection : ITvDbConnection
    {
        private readonly IHttpClient _httpClient;
        private readonly IJsonSerializer _jsonSerialiser;

        public TvDbConnection(IHttpClient httpClient, IJsonSerializer jsonSerialiser)
        {
            _httpClient = httpClient;
            _jsonSerialiser = jsonSerialiser;
        }

        public async Task<RequestResult<TResponseData>> PostAsync<TResponseData>(PostRequest<TResponseData> request)
        {
            var requestOptions = new HttpRequestOptions
            {
                AcceptHeader = "application/json",
                Url = request.Url,
                RequestContent = _jsonSerialiser.SerializeToString(request.Data),
                RequestContentType = "application/json"
            };

            var response = await _httpClient.Post(requestOptions);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var content = new StreamReader(response.Content).ReadToEnd();

                return new RequestResult<TResponseData>(new FailedRequest(response.StatusCode, content));
            }

            var responseData = _jsonSerialiser.DeserializeFromStream<TResponseData>(response.Content);

            return new RequestResult<TResponseData>(new Response<TResponseData>(responseData));
        }

        public async Task<RequestResult<TResponseData>> GetAsync<TResponseData>(GetRequest<TResponseData> request)
        {
            var requestOptions = new HttpRequestOptions
            {
                AcceptHeader = "application/json",
                Url = request.Url
            };

            var response = await _httpClient.GetResponse(requestOptions);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var content = new StreamReader(response.Content).ReadToEnd();

                return new RequestResult<TResponseData>(new FailedRequest(response.StatusCode, content));
            }

            var responseData = _jsonSerialiser.DeserializeFromStream<TResponseData>(response.Content);

            return new RequestResult<TResponseData>(new Response<TResponseData>(responseData));
        }
    }
}