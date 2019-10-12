using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Logging;

namespace MediaBrowser.Plugins.AniMetadata.Files
{
    using Infrastructure;

    internal class FileDownloader : IFileDownloader
    {
        private readonly IHttpClient httpClient;
        private readonly ILogger log;
        private readonly IRateLimiter requestLimiter;

        public FileDownloader(IRateLimiters rateLimiters, IHttpClient httpClient, ILogManager logManager)
        {
            this.httpClient = httpClient;
            this.log = logManager.GetLogger(nameof(FileDownloader));
            this.requestLimiter = rateLimiters.AniDb;
        }

        public async Task DownloadFileAsync<T>(IRemoteFileSpec<T> fileSpec, CancellationToken cancellationToken)
            where T : class
        {
            await this.requestLimiter.TickAsync();

            var requestOptions = new HttpRequestOptions
            {
                Url = fileSpec.Url,
                CancellationToken = cancellationToken,
                EnableHttpCompression = false
            };

            using (var stream = await this.httpClient.Get(requestOptions).ConfigureAwait(false))
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

                    this.log.Debug($"Saving {text.Length} characters to {fileSpec.LocalPath}");

                    text = text.Replace("&#x0;", string.Empty);

                    await writer.WriteAsync(text).ConfigureAwait(false);

                    await writer.FlushAsync().ConfigureAwait(false);
                }
            }
        }
    }
}