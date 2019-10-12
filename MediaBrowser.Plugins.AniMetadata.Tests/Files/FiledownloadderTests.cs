using MediaBrowser.Plugins.AniMetadata.AniDb.Titles;
using MediaBrowser.Plugins.AniMetadata.Files;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.AniMetadata.Tests.Files
{
    [TestFixture]
    public class FileDownloadderTests
    {
        private IFileDownloader fileDownloader;

        [SetUp]
        public void Setup()
        {
            this.fileDownloader = DependencyConfiguration.Resolve<IFileDownloader>(new TestApplicationHost());
        }

        [Test]
        [Ignore("Anidb ip ban")]
        public async Task DownloadFileAsync_AnidbTitleListDowloaded()
        {
            await this.fileDownloader.DownloadFileAsync(new TitlesFileSpec(@"D:\GitHub\MediaBrowser.Plugins.AniDbForTvDb\MediaBrowser.Plugins.AniMetadata\bin\Debug\netstandard2.0"), new CancellationToken());
        }
    }
}
