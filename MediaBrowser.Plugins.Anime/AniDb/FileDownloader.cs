using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    internal class FileDownloader
    {
        private readonly IHttpClient _httpClient;
        private readonly RateLimiter _requestLimiter;

        public FileDownloader(IHttpClient httpClient)
        {
            _httpClient = httpClient;
            _requestLimiter = new RateLimiter(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5),
                TimeSpan.FromMinutes(5));
        }

        public async Task DownloadFileAsync(string url, string destinationFilePath, CancellationToken cancellationToken)
        {
            await _requestLimiter.Tick();

            var requestOptions = new HttpRequestOptions
            {
                Url = url,
                CancellationToken = cancellationToken,
                EnableHttpCompression = false
            };

            using (var stream = await _httpClient.Get(requestOptions).ConfigureAwait(false))
            using (var unzipped = new GZipStream(stream, CompressionMode.Decompress))
            using (var reader = new StreamReader(unzipped, Encoding.UTF8, true))
            using (var file = File.Open(destinationFilePath, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(file))
            {
                var text = await reader.ReadToEndAsync().ConfigureAwait(false);
                text = text.Replace("&#x0;", "");

                await writer.WriteAsync(text).ConfigureAwait(false);
            }
        }
    }
}