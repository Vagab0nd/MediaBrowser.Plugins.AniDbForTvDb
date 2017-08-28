using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Logging;

namespace MediaBrowser.Plugins.Anime.Files
{
    internal class FileDownloader : IFileDownloader
    {
        private readonly IHttpClient _httpClient;
        private readonly ILogger _log;
        private readonly IRateLimiter _requestLimiter;

        public FileDownloader(IRateLimiters rateLimiters, IHttpClient httpClient, ILogManager logManager)
        {
            _httpClient = httpClient;
            _log = logManager.GetLogger(nameof(FileDownloader));
            _requestLimiter = rateLimiters.AniDb;
        }

        public async Task DownloadFileAsync<T>(IRemoteFileSpec<T> fileSpec, CancellationToken cancellationToken)
            where T : class
        {
            await _requestLimiter.TickAsync();

            var requestOptions = new HttpRequestOptions
            {
                Url = fileSpec.Url,
                CancellationToken = cancellationToken,
                EnableHttpCompression = false
            };

            using (var stream = await _httpClient.Get(requestOptions).ConfigureAwait(false))
            {
                var unzippedStream = stream;

                if (fileSpec.IsGZipped)
                {
                    unzippedStream = new GZipStream(stream, CompressionMode.Decompress);
                }

                using (var reader = new StreamReader(unzippedStream, Encoding.UTF8, true))
                using (var file = File.Open(fileSpec.LocalPath, FileMode.Create, FileAccess.Write))
                using (var writer = new StreamWriter(file))
                {
                    var text = await reader.ReadToEndAsync().ConfigureAwait(false);

                    _log.Debug($"Saving {text.Length} characters to {fileSpec.LocalPath}");

                    text = text.Replace("&#x0;", "");

                    await writer.WriteAsync(text).ConfigureAwait(false);

                    await writer.FlushAsync().ConfigureAwait(false);
                }
            }
        }
    }
}