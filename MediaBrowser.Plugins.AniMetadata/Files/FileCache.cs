using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.Files
{
    internal class FileCache : IFileCache
    {
        private readonly IFileDownloader fileDownloader;
        private readonly IXmlSerialiser serializer;

        public FileCache(IFileDownloader fileDownloader, IXmlSerialiser serializer)
        {
            this.fileDownloader = fileDownloader;
            this.serializer = serializer;
        }

        public Option<T> GetFileContent<T>(ILocalFileSpec<T> fileSpec) where T : class
        {
            var cacheFile = new FileInfo(fileSpec.LocalPath);

            if (!cacheFile.Exists)
            {
                return Option<T>.None;
            }

            return fileSpec.Serialiser.Deserialise<T>(File.ReadAllText(cacheFile.FullName));
        }

        public async Task<Option<T>> GetFileContentAsync<T>(IRemoteFileSpec<T> fileSpec,
            CancellationToken cancellationToken) where T : class
        {
            var cacheFile = new FileInfo(fileSpec.LocalPath);

            if (IsRefreshRequired(cacheFile))
            {
                CreateDirectoryIfNotExists(cacheFile.DirectoryName);

                ClearCacheFilesFromDirectory(cacheFile.DirectoryName);

                await DownloadFileAsync(fileSpec, cancellationToken);
            }

            return this.serializer.Deserialise<T>(File.ReadAllText(cacheFile.FullName));
        }

        public void SaveFile<T>(ILocalFileSpec<T> fileSpec, T data) where T : class
        {
            CreateDirectoryIfNotExists(Path.GetDirectoryName(fileSpec.LocalPath));

            var serialized = fileSpec.Serialiser.Serialise(data);

            File.WriteAllText(fileSpec.LocalPath, serialized);
        }

        private async Task DownloadFileAsync<T>(IRemoteFileSpec<T> fileSpec, CancellationToken cancellationToken)
            where T : class
        {
            await this.fileDownloader.DownloadFileAsync(fileSpec, cancellationToken);
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