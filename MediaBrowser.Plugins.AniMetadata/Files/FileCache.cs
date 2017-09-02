using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Functional.Maybe;

namespace MediaBrowser.Plugins.AniMetadata.Files
{
    internal class FileCache : IFileCache
    {
        private readonly IFileDownloader _fileDownloader;
        private readonly IXmlSerialiser _serialiser;

        public FileCache(IFileDownloader fileDownloader, IXmlSerialiser serialiser)
        {
            _fileDownloader = fileDownloader;
            _serialiser = serialiser;
        }

        public Maybe<T> GetFileContent<T>(ILocalFileSpec<T> fileSpec) where T : class
        {
            var cacheFile = new FileInfo(fileSpec.LocalPath);

            if (!cacheFile.Exists)
            {
                return Maybe<T>.Nothing;
            }

            return fileSpec.Serialiser.Deserialise<T>(File.ReadAllText(cacheFile.FullName)).ToMaybe();
        }

        public async Task<Maybe<T>> GetFileContentAsync<T>(IRemoteFileSpec<T> fileSpec,
            CancellationToken cancellationToken) where T : class
        {
            var cacheFile = new FileInfo(fileSpec.LocalPath);

            if (IsRefreshRequired(cacheFile))
            {
                CreateDirectoryIfNotExists(cacheFile.DirectoryName);

                ClearCacheFilesFromDirectory(cacheFile.DirectoryName);

                await DownloadFileAsync(fileSpec, cancellationToken);
            }

            return _serialiser.Deserialise<T>(File.ReadAllText(cacheFile.FullName)).ToMaybe();
        }

        public void SaveFile<T>(ILocalFileSpec<T> fileSpec, T data) where T : class
        {
            CreateDirectoryIfNotExists(Path.GetDirectoryName(fileSpec.LocalPath));

            var serialised = fileSpec.Serialiser.Serialise(data);

            File.WriteAllText(fileSpec.LocalPath, serialised);
        }

        private async Task DownloadFileAsync<T>(IRemoteFileSpec<T> fileSpec, CancellationToken cancellationToken)
            where T : class
        {
            await _fileDownloader.DownloadFileAsync(fileSpec, cancellationToken);
        }

        private void CreateDirectoryIfNotExists(string directoryPath)
        {
            var titlesDirectoryExists = Directory.Exists(directoryPath);
            if (!titlesDirectoryExists)
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        private void ClearCacheFilesFromDirectory(string directoryPath)
        {
            try
            {
                foreach (var file in Directory.GetFiles(directoryPath, "*.xml", SearchOption.AllDirectories))
                    File.Delete(file);

                foreach (var file in Directory.GetFiles(directoryPath, "*.json", SearchOption.AllDirectories))
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
    }
}