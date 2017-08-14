using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Plugins.Anime.AniDb;
using MediaBrowser.Plugins.Anime.AniDb.Mapping;
using MediaBrowser.Plugins.Anime.Mapping.Data;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.Anime.Tests
{
    [TestFixture]
    public class AnimeMappingListFactoryTests
    {
        private static string MappingsFilePath => AppDomain.CurrentDomain.BaseDirectory +
            @"\TestData\Mappings\anime-list.xml";

        [Test]
        public async Task CreateMappingListAsync_ParsesFileCorrectly()
        {
            var applicationPaths = Substitute.For<IApplicationPaths>();
            var fileCache = Substitute.For<IAniDbFileCache>();
            var fileParser = new XmlFileParser();

            fileCache.GetFileAsync(null, CancellationToken.None).ReturnsForAnyArgs(new FileInfo(MappingsFilePath));

            var factory = new AnimeMappingListFactory(applicationPaths, fileCache, fileParser);

            var mappingList = await factory.CreateMappingListAsync(CancellationToken.None);


            mappingList.AnimeSeriesMapping.Length.Should().Be(7427);
            mappingList.AnimeSeriesMapping[22].ShouldBeEquivalentTo(new AnimeSeriesMapping
            {
                AnidbId = "23",
                Name = "Cowboy Bebop",
                TvDbId = "76885",
                DefaultTvDbSeason = "1",
                GroupMappingList = new[]
                {
                    new AnimeEpisodeGroupMapping
                    {
                        Value = ";1-2;",
                        AnidbSeason = 0,
                        TvDbSeason = 0
                    }
                },
                SupplementalInfo = new[]
                {
                    new AnimeSeriesSupplementalInfo
                    {
                        Items = new object[]
                        {
                            "Sunrise"
                        },
                        ItemsElementName = new[]
                        {
                            ItemsChoiceType.studio
                        }
                    }
                }
            });

            mappingList.AnimeSeriesMapping[101].ShouldBeEquivalentTo(new AnimeSeriesMapping
            {
                AnidbId = "107",
                Name = "Chikyuu Shoujo Arjuna",
                TvDbId = "80113",
                DefaultTvDbSeason = "1",
                GroupMappingList = new[]
                {
                    new AnimeEpisodeGroupMapping
                    {
                        Value = ";1-9;",
                        AnidbSeason = 0,
                        TvDbSeason = 1
                    },
                    new AnimeEpisodeGroupMapping
                    {
                        Value = null,
                        AnidbSeason = 1,
                        TvDbSeason = 1,
                        Start = 9,
                        End = 12,
                        Offset = 1,
                        StartSpecified = true,
                        EndSpecified = true,
                        OffsetSpecified = true
                    }
                },
                Before = ";1-9;"
            });
        }
    }
}