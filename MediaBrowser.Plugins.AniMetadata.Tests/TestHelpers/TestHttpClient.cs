using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;

namespace MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers
{
    internal class TestHttpClient : IHttpClient
    {
        private readonly HttpClient _client;

        public TestHttpClient()
        {
            _client = new HttpClient();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public async Task<HttpResponseInfo> GetResponse(HttpRequestOptions options)
        {
            _client.DefaultRequestHeaders.Clear();

            foreach (var optionsRequestHeader in options.RequestHeaders)
                _client.DefaultRequestHeaders.Add(optionsRequestHeader.Key, optionsRequestHeader.Value);

            try
            {
                var response = await _client.GetStreamAsync(options.Url);

                return new HttpResponseInfo
                {
                    Content = response,
                    StatusCode = HttpStatusCode.OK
                };
            }
            catch
            {
                return new HttpResponseInfo
                {
                    Content = null,
                    StatusCode = HttpStatusCode.NotFound
                };
            }
        }

        public Task<Stream> Get(string url, SemaphoreSlim resourcePool, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> Get(string url, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> Get(HttpRequestOptions options)
        {
            throw new NotImplementedException();
        }

        public Task<HttpResponseInfo> SendAsync(HttpRequestOptions options, string httpMethod)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> Post(string url, Dictionary<string, string> postData, SemaphoreSlim resourcePool,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> Post(string url, Dictionary<string, string> postData, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> Post(HttpRequestOptions options, Dictionary<string, string> postData)
        {
            throw new NotImplementedException();
        }

        public async Task<HttpResponseInfo> Post(HttpRequestOptions options)
        {
            _client.DefaultRequestHeaders.Clear();

            foreach (var optionsRequestHeader in options.RequestHeaders)
                _client.DefaultRequestHeaders.Add(optionsRequestHeader.Key, optionsRequestHeader.Value);

            var response = await _client.PostAsync(options.Url,
                new StringContent(options.RequestContent, Encoding.UTF8, options.RequestContentType));

            var responseContent = await response.Content.ReadAsStreamAsync();

            return new HttpResponseInfo
            {
                Content = responseContent,
                StatusCode = response.StatusCode
            };
        }

        public Task<string> GetTempFile(HttpRequestOptions options)
        {
            throw new NotImplementedException();
        }

        public Task<HttpResponseInfo> GetTempFileResponse(HttpRequestOptions options)
        {
            throw new NotImplementedException();
        }
    }
}