using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Serialization;

namespace MediaBrowser.Plugins.Anime.TvDb
{
    internal class TvDbConnection
    {
        private readonly IHttpClient _httpClient;
        private readonly IJsonSerializer _jsonSerialiser;

        public TvDbConnection(IHttpClient httpClient, IJsonSerializer jsonSerialiser)
        {
            _httpClient = httpClient;
            _jsonSerialiser = jsonSerialiser;
        }

        public async Task<TResponse> PostAsync<TResponse>(PostRequest<TResponse> request)
        {
            var requestOptions = new HttpRequestOptions
            {
                AcceptHeader = "application/json",
                Url = request.Url,
                RequestContent = _jsonSerialiser.SerializeToString(request.Data),
                RequestContentType = "application/json"
            };

            var response = await _httpClient.Post(requestOptions);

            return _jsonSerialiser.DeserializeFromStream<TResponse>(response.Content);
        }
    }
}