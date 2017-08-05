using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    internal class AniDbFileCache
    {
        private const string ClientName = "mediabrowser";
        private const string SeriesPath = "anidb\\series";

        private const string SeriesQueryUrl =
            "http://api.anidb.net:9001/httpapi?request=anime&client={0}&clientver=1&protover=1&aid={1}";

        private readonly RateLimiter _requestLimiter;
        private readonly string _rootPath;

        public AniDbFileCache(IApplicationPaths applicationPaths)
        {
            _requestLimiter = new RateLimiter(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5),
                TimeSpan.FromMinutes(5));
            _rootPath = applicationPaths.CachePath;
        }

        public async Task<FileInfo> GetSeriesFileAsync(string aniDbSeriesId, IHttpClient httpClient,
            CancellationToken cancellationToken)
        {
            var cacheFile = new FileInfo(GetSeriesCacheFilePath(aniDbSeriesId));

            if (!IsRefreshRequired(cacheFile))
            {
                return cacheFile;
            }

            var seriesDirectoryExists = Directory.Exists(cacheFile.DirectoryName);
            if (!seriesDirectoryExists)
            {
                Directory.CreateDirectory(cacheFile.DirectoryName);
            }

            ClearCache(cacheFile.DirectoryName);

            await _requestLimiter.Tick();

            await DownloadFileAsync(aniDbSeriesId, httpClient, cancellationToken);

            return cacheFile;
        }

        private void ClearCache(string directoryPath)
        {
            try
            {
                foreach (var file in Directory.GetFiles(directoryPath, "*.xml", SearchOption.AllDirectories))
                    File.Delete(file);
            }
            catch (DirectoryNotFoundException)
            {
            }
        }

        private bool IsRefreshRequired(FileInfo cacheFile)
        {
            return !cacheFile.Exists ||
                cacheFile.LastWriteTime < DateTime.Now.AddDays(-7);
        }

        private string GetSeriesCacheFilePath(string aniDbSeriesId)
        {
            return Path.Combine(_rootPath, SeriesPath, aniDbSeriesId, "series.xml");
        }

        private async Task DownloadFileAsync(string aniDbSeriesId, IHttpClient httpClient,
            CancellationToken cancellationToken)
        {
            var requestOptions = new HttpRequestOptions
            {
                Url = string.Format(SeriesQueryUrl, ClientName, aniDbSeriesId),
                CancellationToken = cancellationToken,
                EnableHttpCompression = false
            };

            using (var stream = await httpClient.Get(requestOptions).ConfigureAwait(false))
            using (var unzipped = new GZipStream(stream, CompressionMode.Decompress))
            using (var reader = new StreamReader(unzipped, Encoding.UTF8, true))
            using (var file = File.Open(SeriesPath, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(file))
            {
                var text = await reader.ReadToEndAsync().ConfigureAwait(false);
                text = text.Replace("&#x0;", "");

                await writer.WriteAsync(text).ConfigureAwait(false);
            }
        }
    }
}