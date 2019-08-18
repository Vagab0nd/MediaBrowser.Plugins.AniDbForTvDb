using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.EntryPoints;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests.IntegrationTests
{
    [TestFixture]
    public class SeriesProviderIntegrationTests
    {
        [SetUp]
        public void Setup()
        {
            _applicationHost = new TestApplicationHost();
            var applicationPaths = _applicationHost.Resolve<IApplicationPaths>();

            var plugin = new Plugin(applicationPaths,
                _applicationHost.Resolve<IXmlSerializer>());

            plugin.SetConfiguration(new PluginConfiguration
            {
                TvDbApiKey = Secrets.TvDbApiKey
            });

            // pre-populate the cache to avoid spamming AniDb with requests when the tests run
            // (and for static test data)
            FileCacheHelper.SetupCachedFile(applicationPaths.CachePath, @"\anidb\titles.xml",
                @"\anidb\titles\titles.xml");

            FileCacheHelper.SetupCachedFile(applicationPaths.CachePath, @"\anidb\959.xml",
                @"\anidb\series\959\series.xml");

            FileCacheHelper.SetupCachedFile(applicationPaths.CachePath, @"\TvDb\78914.json", @"\anidb\tvdb\78914.json");

            FileCacheHelper.SetupCachedFile(applicationPaths.CachePath, @"\Mappings\anime-list.xml",
                @"\anime-list.xml");
        }

        private TestApplicationHost _applicationHost;

        [Test]
        [TestCase("AniDb")]
        [TestCase("Tvdb")]
        public async Task GetMetadata_AniDbLibraryStructure_UsesNameFromLibraryStructureSource(
            string fileStructureSourceName)
        {
            Plugin.Instance.Configuration.LibraryStructureSourceName = SourceNames.AniDb;

            var seriesInfo = new SeriesInfo
            {
                Name = "Full Metal Panic Fumoffu"
            };

            var seriesEntryPoint = new SeriesProviderEntryPoint(_applicationHost);

            var result = await seriesEntryPoint.GetMetadata(seriesInfo, CancellationToken.None);

            result.HasMetadata.Should().BeTrue();
            result.Item.Should()
                .BeEquivalentTo(new Series
                    {
                        Name = "Fullmetal Panic? Fumoffu",
                        AirDays = new[] { DayOfWeek.Tuesday },
                        AirTime = "18:30",
                        PremiereDate = new DateTime(2003, 08, 26),
                        EndDate = new DateTime(2003, 11, 18),
                        Overview =
                            @"It is back-to-school mayhem with Chidori Kaname and her battle-hardened classmate Sagara Sousuke as they encounter more misadventures in and out of Jindai High School. But when Kaname gets into some serious trouble, Sousuke takes the guise of Bonta-kun — the gun-wielding, butt-kicking mascot. And while he struggles to continue living as a normal teenager, Sousuke also has to deal with protecting his superior officer Teletha Testarossa, who has decided to take a vacation from Mithril and spend a couple of weeks as his and Kaname`s classmate.
Source: ANN
Note: Because of a then current kidnapping event, TV Tokyo did not broadcast what were supposed to be part 2 of episode 1 (A Hostage with No Compromises) and part 1 of episode 2 (Hostility Passing-By). A Hostage with No Compromises was replaced by part 2 of episode 2 (A Fruitless Lunchtime) and thus, episode 2 was skipped, leaving the episode count at 11. The DVD release contained all the episodes in the intended order.",
                        Studios = new[] { "Kyoto Animation" },
                        Genres = new[] { "Anime", "Present", "Earth", "Slapstick", "Japan" },
                        Tags = new[] { "Asia", "Comedy", "High School", "School Life", "Action" },
                        CommunityRating = 8.22f,
                        ProviderIds =
                            new Dictionary<string, string>
                            {
                                { SourceNames.AniDb, "959" },
                                { SourceNames.TvDb, "78914" }
                            }
                    },
                    o => o.Excluding(s => s.DisplayPreferencesId)
                        //.Excluding(s => s.Children)
                        //.Excluding(s => s.RecursiveChildren)
                        .Excluding(s => s.SortName));
            result.People.Should().HaveCount(61);
        }

        [Test]
        [TestCase("AniDb")]
        [TestCase("Tvdb")]
        public async Task GetMetadata_AniListMappings_MapsAniListData(string fileStructureSourceName)
        {
            Plugin.Instance.Configuration.AniListAccessToken =
                "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImp0aSI6IjA1YzRjZDMyNWY1MGNhMTllMThjMzU5MmI3YmQ3NzczNjUzYzVkNzdkNGY3NmYzZDcyNDY5ZDVkNmFhMGE3YmFmYjg2MjM3YjcxM2M2ZjAxIn0.eyJhdWQiOiIzNjIiLCJqdGkiOiIwNWM0Y2QzMjVmNTBjYTE5ZTE4YzM1OTJiN2JkNzc3MzY1M2M1ZDc3ZDRmNzZmM2Q3MjQ2OWQ1ZDZhYTBhN2JhZmI4NjIzN2I3MTNjNmYwMSIsImlhdCI6MTUyMzExMDA5NywibmJmIjoxNTIzMTEwMDk3LCJleHAiOjE1NTQ2NDYwOTcsInN1YiI6IjExMjA4NCIsInNjb3BlcyI6W119.eXUQ1VrEQdinxvuphdPxTmNgISnBf2sYUOdi3bhsR6Rp0_Tohh3PzKXEDZKt6Deu3NZieZ_ET5sMb1iYAeTX5K_XHhYOQwcZzGSwstBT84HkyPl6FL6ONrCxO94z4arfnpriNM3eVPhGQee9CT5jEpMxYAtTgN8-9MsDD5pyc_AvRT_AuC2ugqw81dgPCgNDjSAiOSBNG1XWpXI2jV1jF5TKaOVlfedJqCL-scL7j4XBiq3v-2WdPaV5oqw2kvEfH5A5pReIU_m-SAFduAgvPNPdgGSh7izx14WSzdWpuiYLc_ly8VhxptwWnlHifLrAeu0t2UjmCy5Ssh1op2Bmo2qXJPlx9xcdTyW2yqTxxH-V_VbsPH2Omvmda_PFsi6sLKhCEF1qGhAJ0aSGIpbTl8V6tJ4-JxbhU2GjyR13LOHTOIU7sM_OO9ketgKGZ6L2wI4LQGbm6BIop96QweRjT19hCwkwHS-Tq1d0HRtCJ_tPHuupZKARDrMQgkVHTJ1lPsIKyf92KnUJ1azn6AxTSwvxQSVnaiqM_1CMsC0ht0bs9RgnqjAKRv734qt7yibItu7UzzV3JIOYX2duG8KW_VLyM78V4DqIEhui7K9MARavlqEs2umOkLHb1aaLz9zVvgrNQrzwUXeXPQm_myWgieWEP5RIQH7Gv_YC-W1pB0o";

            Plugin.Instance.Configuration.LibraryStructureSourceName = SourceNames.AniDb;
            Plugin.Instance.Configuration.SeriesMappings.Iter(sm =>
            {
                if (sm.Mappings.Any(m => m.SourceName == SourceNames.AniList))
                {
                    sm.Mappings = sm.Mappings.Where(m =>
                            m.SourceName == SourceNames.AniList || m.TargetPropertyName == nameof(Series.Name))
                        .ToArray();
                }
            });

            var seriesInfo = new SeriesInfo
            {
                Name = "Full Metal Panic Fumoffu"
            };

            var seriesEntryPoint = new SeriesProviderEntryPoint(_applicationHost);

            var result = await seriesEntryPoint.GetMetadata(seriesInfo, CancellationToken.None);

            result.HasMetadata.Should().BeTrue();
            result.Item.Should()
                .BeEquivalentTo(new Series
                    {
                        Name = "Fullmetal Panic? Fumoffu",
                        AirDays = new[] { DayOfWeek.Tuesday },
                        AirTime = "18:30",
                        PremiereDate = new DateTime(2003, 08, 26),
                        EndDate = new DateTime(2003, 10, 18),
                        Overview =
                            "It's back-to-school mayhem with Kaname Chidori and her war-freak classmate Sousuke Sagara as they encounter more misadventures in and out of Jindai High School. But when Kaname gets into some serious trouble, Sousuke takes the guise of Bonta-kun - the gun-wielding, butt-kicking mascot. And while he struggles to continue living as a normal teenager, Sousuke also has to deal with protecting his superior officer Teletha Testarossa, who has decided to take a vacation from Mithril and spend a couple of weeks as his and Kaname's classmate.<br><br>\n(Source: ANN)",
                        Studios = new[] { "Kyoto Animation", "ADV Films", "FUNimation Entertainment" },
                        Genres = new [] { "Action", "Comedy" },
                        Tags = new string[] { },
                        CommunityRating = 78f,
                        ProviderIds = new Dictionary<string, string>
                        {
                            { SourceNames.AniDb, "959" },
                            { SourceNames.TvDb, "78914" },
                            { SourceNames.AniList, "72" }
                        }
                    },
                    o => o.Excluding(s => s.DisplayPreferencesId)
                        //.Excluding(s => s.Children)
                        //.Excluding(s => s.RecursiveChildren)
                        .Excluding(s => s.SortName));
            result.People.Should().HaveCount(48);
        }

        [Test]
        [TestCase("AniDb")]
        [TestCase("Tvdb")]
        public async Task GetMetadata_TvDbLibraryStructure_UsesNameFromLibraryStructureSource(
            string fileStructureSourceName)
        {
            Plugin.Instance.Configuration.LibraryStructureSourceName = SourceNames.TvDb;

            var seriesInfo = new SeriesInfo
            {
                Name = "Full Metal Panic Fumoffu"
            };

            var seriesEntryPoint = new SeriesProviderEntryPoint(_applicationHost);

            var result = await seriesEntryPoint.GetMetadata(seriesInfo, CancellationToken.None);

            result.HasMetadata.Should().BeTrue();
            result.Item.Should()
                .BeEquivalentTo(new Series
                    {
                        Name = "Full Metal Panic!",
                        AirDays = new[] { DayOfWeek.Tuesday },
                        AirTime = "18:30",
                        PremiereDate = new DateTime(2003, 08, 26),
                        EndDate = new DateTime(2003, 11, 18),
                        Overview =
                            @"It is back-to-school mayhem with Chidori Kaname and her battle-hardened classmate Sagara Sousuke as they encounter more misadventures in and out of Jindai High School. But when Kaname gets into some serious trouble, Sousuke takes the guise of Bonta-kun — the gun-wielding, butt-kicking mascot. And while he struggles to continue living as a normal teenager, Sousuke also has to deal with protecting his superior officer Teletha Testarossa, who has decided to take a vacation from Mithril and spend a couple of weeks as his and Kaname`s classmate.
Source: ANN
Note: Because of a then current kidnapping event, TV Tokyo did not broadcast what were supposed to be part 2 of episode 1 (A Hostage with No Compromises) and part 1 of episode 2 (Hostility Passing-By). A Hostage with No Compromises was replaced by part 2 of episode 2 (A Fruitless Lunchtime) and thus, episode 2 was skipped, leaving the episode count at 11. The DVD release contained all the episodes in the intended order.",
                        Studios = new[] { "Kyoto Animation" },
                        Genres = new[] { "Anime", "Present", "Earth", "Slapstick", "Japan" },
                        Tags = new[] { "Asia", "Comedy", "High School", "School Life", "Action" },
                        CommunityRating = 8.22f,
                        ProviderIds =
                            new Dictionary<string, string>
                            {
                                { SourceNames.AniDb, "959" },
                                { SourceNames.TvDb, "78914" }
                            }
                    },
                    o => o.Excluding(s => s.DisplayPreferencesId)
                        //.Excluding(s => s.Children)
                        //.Excluding(s => s.RecursiveChildren)
                        .Excluding(s => s.SortName));
            result.People.Should().HaveCount(61);
        }
    }
}