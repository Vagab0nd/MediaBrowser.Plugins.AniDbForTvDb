using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.Mapping.Data
{
    public class AnimeMappingListFactory
    {
        private const string TempFilePath = "anime-list.xml";

        private readonly AsyncLock _lock = new AsyncLock();
        private readonly string _tempFilePath;

        public AnimeMappingListFactory(string tempFilePath = TempFilePath)
        {
            _tempFilePath = tempFilePath;
        }

        public async Task<AnimeMappingList> CreateMappingListAsync()
        {
            using (await _lock.LockAsync())
            {
                if (LocalFileIsLessThan7DaysOld())
                {
                    await RefreshLocalFileAsync();
                }
            }

            return ReadLocalFile();
        }

        private AnimeMappingList ReadLocalFile()
        {
            var serializer = new XmlSerializer(typeof(AnimeMappingList));

            using (var stream = File.OpenRead(_tempFilePath))
            {
                return serializer.Deserialize(stream) as AnimeMappingList;
            }
        }

        private async Task RefreshLocalFileAsync()
        {
            var info = new FileInfo(_tempFilePath);

            if (info.Exists)
            {
                info.Delete();
            }

            await DownloadFileAsync();
        }

        private bool LocalFileIsLessThan7DaysOld()
        {
            var info = new FileInfo(_tempFilePath);

            return info.Exists && info.LastWriteTimeUtc >= DateTime.UtcNow - TimeSpan.FromDays(7);
        }

        private async Task DownloadFileAsync()
        {
            var client = new WebClient();
            await client.DownloadFileTaskAsync(
                "https://raw.githubusercontent.com/ScudLee/anime-lists/master/anime-list.xml", _tempFilePath);
        }
    }
}