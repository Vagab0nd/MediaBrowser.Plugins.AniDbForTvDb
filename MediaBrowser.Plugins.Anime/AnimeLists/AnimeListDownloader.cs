using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AnimeLists
{
    public class AnimeListDownloader
    {
        private readonly AsyncLock _lock = new AsyncLock();
        private readonly string _tempFilePath;

        public AnimeListDownloader(string tempFilePath)
        {
            _tempFilePath = tempFilePath;
        }

        public async Task<Animelist> DownloadAsync()
        {
            using (await _lock.LockAsync())
            {
                if (LocalFileIsLessThan7DaysOld())
                {
                    await RefreshLocalAnimeListFileAsync();
                }
            }

            return ReadLocalAnimeListFile();
        }

        private Animelist ReadLocalAnimeListFile()
        {
            var serializer = new XmlSerializer(typeof(Animelist));

            using (var stream = File.OpenRead(_tempFilePath))
            {
                return serializer.Deserialize(stream) as Animelist;
            }
        }

        private async Task RefreshLocalAnimeListFileAsync()
        {
            var info = new FileInfo(_tempFilePath);

            if (info.Exists)
            {
                info.Delete();
            }

            await DownloadAnimeList();
        }

        private bool LocalFileIsLessThan7DaysOld()
        {
            var info = new FileInfo(_tempFilePath);

            return info.Exists && info.LastWriteTimeUtc >= DateTime.UtcNow - TimeSpan.FromDays(7);
        }

        private async Task DownloadAnimeList()
        {
            var client = new WebClient();
            await client.DownloadFileTaskAsync(
                "https://raw.githubusercontent.com/ScudLee/anime-lists/master/anime-list.xml", _tempFilePath);
        }
    }
}