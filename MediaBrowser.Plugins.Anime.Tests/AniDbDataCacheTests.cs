using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Functional.Maybe;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Plugins.Anime.AniDb;
using MediaBrowser.Plugins.Anime.AniDb.Seiyuu;
using MediaBrowser.Plugins.Anime.AniDb.Series.Data;
using MediaBrowser.Plugins.Anime.Tests.TestData;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.Anime.Tests
{
    [TestFixture]
    public class AniDbDataCacheTests
    {
        [Test]
        public async Task GetSeries_AddsSeiyuuToFile()
        {
            var rootPath = AppDomain.CurrentDomain.BaseDirectory + @"\" + Guid.NewGuid();
            var expectedFileLocation = rootPath + @"\anidb\seiyuu.xml";
            var applicationPaths = Substitute.For<IApplicationPaths>();
            var fileCache = Substitute.For<IAniDbFileCache>();

            var seiyuu1 = new SeiyuuData
            {
                Id = 3,
                Name = "Test",
                PictureFileName = "132"
            };

            var seiyuu2 = new SeiyuuData
            {
                Id = 4,
                Name = "Test2",
                PictureFileName = "422"
            };

            var series = new AniDbSeriesData().WithStandardData();
            var seriesWithExtraSeiyuu = new AniDbSeriesData().WithStandardData();

            seriesWithExtraSeiyuu.Characters = seriesWithExtraSeiyuu.Characters.Concat(new[]
            {
                new CharacterData
                {
                    Seiyuu = seiyuu1
                },
                new CharacterData
                {
                    Seiyuu = seiyuu2
                }
            }).ToArray();

            applicationPaths.CachePath.Returns(rootPath);

            fileCache.GetFileContentAsync(Arg.Is<SeriesFileSpec>(s => s.Url.EndsWith("1")),
                Arg.Any<CancellationToken>()).Returns(series.ToMaybe());

            fileCache.GetFileContentAsync(Arg.Is<SeriesFileSpec>(s => s.Url.EndsWith("2")),
                Arg.Any<CancellationToken>()).Returns(seriesWithExtraSeiyuu.ToMaybe());

            var aniDbDataCache = new AniDbDataCache(applicationPaths, fileCache);

            aniDbDataCache.GetSeiyuu().Should().BeEmpty();

            await aniDbDataCache.GetSeriesAsync(1, CancellationToken.None);

            fileCache.Received(1).SaveFile(Arg.Is<SeiyuuFileSpec>(f => f.LocalPath == expectedFileLocation), Arg.Is<SeiyuuListData>(s => s.Seiyuu.Length == 1));

            fileCache.GetFileContent<SeiyuuListData>(null)
                .ReturnsForAnyArgs(new SeiyuuListData { Seiyuu = new[] { series.Characters[0].Seiyuu } }.ToMaybe());

            await aniDbDataCache.GetSeriesAsync(2, CancellationToken.None);
            
            fileCache.Received(1).SaveFile(Arg.Is<SeiyuuFileSpec>(f => f.LocalPath == expectedFileLocation), Arg.Is<SeiyuuListData>(s => s.Seiyuu.SequenceEqual(new[] { series.Characters[0].Seiyuu, seiyuu1, seiyuu2 })));
        }

        [Test]
        public async Task GetSeries_SavesSeiyuuFile()
        {
            var rootPath = AppDomain.CurrentDomain.BaseDirectory + @"\" + Guid.NewGuid();
            var expectedFileLocation = rootPath + @"\anidb\seiyuu.xml";
            var applicationPaths = Substitute.For<IApplicationPaths>();
            var fileCache = Substitute.For<IAniDbFileCache>();

            applicationPaths.CachePath.Returns(rootPath);

            fileCache.GetFileContentAsync(Arg.Is<SeriesFileSpec>(s => s.Url.EndsWith("1")),
                Arg.Any<CancellationToken>()).Returns(new AniDbSeriesData().WithStandardData().ToMaybe());

            var aniDbDataCache = new AniDbDataCache(applicationPaths, fileCache);

            await aniDbDataCache.GetSeriesAsync(1, CancellationToken.None);

            fileCache.Received(1).SaveFile(Arg.Is<SeiyuuFileSpec>(f => f.LocalPath == expectedFileLocation), Arg.Is<SeiyuuListData>(s => s.Seiyuu.Length == 1));
        }

        [Test]
        public void GetSeiyuu_LoadsSeiyuuFile()
        {
            var rootPath = AppDomain.CurrentDomain.BaseDirectory + @"\" + Guid.NewGuid();
            var expectedFileLocation = rootPath + @"\anidb\seiyuu.xml";
            var applicationPaths = Substitute.For<IApplicationPaths>();
            var fileCache = Substitute.For<IAniDbFileCache>();

            applicationPaths.CachePath.Returns(rootPath);
            
            var aniDbDataCache = new AniDbDataCache(applicationPaths, fileCache);

            aniDbDataCache.GetSeiyuu();

            fileCache.Received(1).GetFileContent(Arg.Is<SeiyuuFileSpec>(f => f.LocalPath == expectedFileLocation));
        }
    }
}