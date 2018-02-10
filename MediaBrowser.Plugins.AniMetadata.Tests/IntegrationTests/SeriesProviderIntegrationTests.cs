using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.EntryPoints;
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
            Directory.CreateDirectory(applicationPaths.CachePath + @"\anidb\titles");
            File.Copy(AppDomain.CurrentDomain.BaseDirectory + @"\TestData\anidb\titles.xml",
                applicationPaths.CachePath + @"\anidb\titles\titles.xml");

            Directory.CreateDirectory(applicationPaths.CachePath + @"\anidb\series\959");
            File.Copy(AppDomain.CurrentDomain.BaseDirectory + @"\TestData\anidb\959.xml",
                applicationPaths.CachePath + @"\anidb\series\959\series.xml");

            Directory.CreateDirectory(applicationPaths.CachePath);
            File.Copy(AppDomain.CurrentDomain.BaseDirectory + @"\TestData\Mappings\anime-list.xml",
                applicationPaths.CachePath + @"\anime-list.xml");
        }

        private TestApplicationHost _applicationHost;

        [Test]
        [TestCase("AniDb")]
        [TestCase("TvDb")]
        public async Task GetMetadata_AniDbLibraryStructure_UsesNameFromLibraryStructureSource(string fileStructureSourceName)
        {
            Plugin.Instance.Configuration.LibraryStructureSourceName = "AniDb";

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
                        Genres = new List<string> { "Anime", "Present", "Earth", "Slapstick", "Japan" },
                        Tags = new[] { "Asia", "Comedy", "High School", "School Life", "Action" },
                        CommunityRating = 8.22f,
                        ProviderIds = new Dictionary<string, string> { { "AniDb", "959" }, { "TvDb", "78914" } }
                    },
                    o => o.Excluding(s => s.DisplayPreferencesId)
                        .Excluding(s => s.Children)
                        .Excluding(s => s.RecursiveChildren)
                        .Excluding(s => s.SortName));
            result.People.Should().HaveCount(61);
        }

        [Test]
        [TestCase("AniDb")]
        [TestCase("TvDb")]
        public async Task GetMetadata_TvDbLibraryStructure_UsesNameFromLibraryStructureSource(string fileStructureSourceName)
        {
            Plugin.Instance.Configuration.LibraryStructureSourceName = "TvDb";

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
                    Genres = new List<string> { "Anime", "Present", "Earth", "Slapstick", "Japan" },
                    Tags = new[] { "Asia", "Comedy", "High School", "School Life", "Action" },
                    CommunityRating = 8.22f,
                    ProviderIds = new Dictionary<string, string> { { "AniDb", "959" }, { "TvDb", "78914" } }
                },
                    o => o.Excluding(s => s.DisplayPreferencesId)
                        .Excluding(s => s.Children)
                        .Excluding(s => s.RecursiveChildren)
                        .Excluding(s => s.SortName));
            result.People.Should().HaveCount(61);
        }


    }
}