using System;
using System.IO;
using FluentAssertions;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Plugins.Anime.AniDb;
using MediaBrowser.Plugins.Anime.AniDb.Data;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.Anime.Tests
{
    [TestFixture]
    public class SeiyuuCacheTests
    {
        [Test]
        public void Add_ExistingFile_UpdatesFile()
        {
            var rootPath = AppDomain.CurrentDomain.BaseDirectory + @"\" + Guid.NewGuid();
            var applicationPaths = Substitute.For<IApplicationPaths>();

            applicationPaths.CachePath.Returns(rootPath);

            var seiyuuCache = new SeiyuuCache(new AniDbFileParser(), applicationPaths);

            var seiyuu1 = new Seiyuu
            {
                Id = 1,
                Name = "Test",
                PictureFileName = "132"
            };

            var seiyuu2 = new Seiyuu
            {
                Id = 2,
                Name = "Test2",
                PictureFileName = "422"
            };

            seiyuuCache.Add(new[] { seiyuu1 });

            seiyuuCache.Add(new[] { seiyuu2 });

            seiyuuCache.GetAll().ShouldBeEquivalentTo(new[] { seiyuu1, seiyuu2 });
        }

        [Test]
        public void Add_SavesFile()
        {
            var rootPath = AppDomain.CurrentDomain.BaseDirectory + @"\" + Guid.NewGuid();
            var expectedFileLocation = rootPath + @"\anidb\seiyuu.xml";
            var applicationPaths = Substitute.For<IApplicationPaths>();

            applicationPaths.CachePath.Returns(rootPath);

            var seiyuuCache = new SeiyuuCache(new AniDbFileParser(), applicationPaths);

            var seiyuu = new Seiyuu
            {
                Id = 1,
                Name = "Test",
                PictureFileName = "132"
            };

            seiyuuCache.Add(new[] { seiyuu });

            File.Exists(expectedFileLocation).Should().BeTrue();
        }

        [Test]
        public void GetAll_ReadsAllFromFile()
        {
            var rootPath = AppDomain.CurrentDomain.BaseDirectory + @"\" + Guid.NewGuid();
            var applicationPaths = Substitute.For<IApplicationPaths>();

            applicationPaths.CachePath.Returns(rootPath);

            var seiyuuCache = new SeiyuuCache(new AniDbFileParser(), applicationPaths);

            var seiyuu = new Seiyuu
            {
                Id = 1,
                Name = "Test",
                PictureFileName = "132"
            };

            seiyuuCache.Add(new[] { seiyuu });

            seiyuuCache.GetAll().ShouldBeEquivalentTo(new[] { seiyuu });
        }
    }
}