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
    public class EpisodeProviderIntegrationTests
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

            // pre-populate the cache to avoid spamming sources with requests when the tests run
            // (and for static test data)
            FileCacheHelper.SetupCachedFile(applicationPaths.CachePath, @"\AniDb\titles.xml", @"\anidb\titles\titles.xml");

            FileCacheHelper.SetupCachedFile(applicationPaths.CachePath, @"\AniDb\959.xml", @"\anidb\series\959\series.xml");

            FileCacheHelper.SetupCachedFile(applicationPaths.CachePath, @"\Mappings\anime-list.xml", @"\anime-list.xml");

            FileCacheHelper.SetupCachedFile(applicationPaths.CachePath, @"\TvDb\78914_Episodes.json", @"\anidb\tvdb\78914_Episodes.json");
        }

        private TestApplicationHost _applicationHost;

        [Test]
        [TestCase("AniDb")]
        [TestCase("TvDb")]
        public async Task GetMetadata_AniDbLibraryStructure_UsesNameFromLibraryStructureSource(string fileStructureSourceName)
        {
            Plugin.Instance.Configuration.LibraryStructureSourceName = "AniDb";

            var episodeInfo = new EpisodeInfo
            {
                Name = "The Man from the South A Fruitless Lunchtime",
                SeriesProviderIds = { { "AniDb", "959" } },
                MetadataLanguage = "en"
            };

            var episodeEntryPoint = new EpisodeProviderEntryPoint(_applicationHost);

            var result = await episodeEntryPoint.GetMetadata(episodeInfo, CancellationToken.None);

            result.HasMetadata.Should().BeTrue();
            result.Item.Should()
                .BeEquivalentTo(new Episode
                {
                    Name = "The Man from the South / A Fruitless Lunchtime (AniDb)",
                    PremiereDate = new DateTime(2003, 08, 26),
                    Overview =
                            @"The Man From The South:
A secret admirer leaves a love letter in Sousuke's shoe locker. Instead of finding the letter, he deduces that his locker was tampered with and promptly blows it up. From its fragments, Sousuke misinterprets the letter as a death threat and confronts his ""stalker"".

A Fruitless Lunchtime:
After Sousuke causes a panic at the school's bread stand, he and Kaname have to fill in for the stand's lunchtime duties, which raises the ire of Mr. Kogure, the physical education teacher. He attempts to sabotage their bread, but falls for Sousuke's booby traps, expecting that someone would tamper the bread.",

                    CommunityRating = 8.99f,
                    ProviderIds = new Dictionary<string, string> { { "AniDb", "10407" }, { "TvDb", "1973471" } },
                    RunTimeTicks = 15000000000L
                },
                    o => o.Excluding(s => s.DisplayPreferencesId)
                        .Excluding(s => s.SortName)
                        .Excluding(s => s.SupportsRemoteImageDownloading)
                        .Excluding(s => s.IsMissingEpisode)
                        .Excluding(s => s.SourceType)
                        .Excluding(s => s.IsCompleteMedia)
                        .Excluding(s => s.ContainingFolderPath)
                        .Excluding(s => s.FileNameWithoutExtension)
                        .Excluding(s => s.IsOwnedItem)
                        .Excluding(s => s.LocationType)
                        .Excluding(s => s.SupportsLocalMetadata)
                        .Excluding(s => s.PhysicalLocations));
        }

        [Test]
        [TestCase("AniDb")]
        [TestCase("TvDb")]
        public async Task GetMetadata_TvDbLibraryStructure_UsesNameFromLibraryStructureSource(string fileStructureSourceName)
        {
            Plugin.Instance.Configuration.LibraryStructureSourceName = "TvDb";

            var episodeInfo = new EpisodeInfo
            {
                Name = "The Man from the South A Fruitless Lunchtime",
                SeriesProviderIds = { { "AniDb", "959" } },
                MetadataLanguage = "en"
            };

            var episodeEntryPoint = new EpisodeProviderEntryPoint(_applicationHost);

            var result = await episodeEntryPoint.GetMetadata(episodeInfo, CancellationToken.None);

            result.HasMetadata.Should().BeTrue();
            result.Item.Should()
                .BeEquivalentTo(new Episode
                {
                    Name = "The Man From The South / A Fruitless Lunchtime",
                    PremiereDate = new DateTime(2003, 08, 26),
                    Overview =
                        @"The Man From The South:
A secret admirer leaves a love letter in Sousuke's shoe locker. Instead of finding the letter, he deduces that his locker was tampered with and promptly blows it up. From its fragments, Sousuke misinterprets the letter as a death threat and confronts his ""stalker"".

A Fruitless Lunchtime:
After Sousuke causes a panic at the school's bread stand, he and Kaname have to fill in for the stand's lunchtime duties, which raises the ire of Mr. Kogure, the physical education teacher. He attempts to sabotage their bread, but falls for Sousuke's booby traps, expecting that someone would tamper the bread.",

                    CommunityRating = 8.99f,
                    ProviderIds = new Dictionary<string, string> { { "AniDb", "10407" }, { "TvDb", "1973471" } },
                    RunTimeTicks = 15000000000L
                },
                    o => o.Excluding(s => s.DisplayPreferencesId)
                        .Excluding(s => s.SortName)
                        .Excluding(s => s.SupportsRemoteImageDownloading)
                        .Excluding(s => s.IsMissingEpisode)
                        .Excluding(s => s.SourceType)
                        .Excluding(s => s.IsCompleteMedia)
                        .Excluding(s => s.ContainingFolderPath)
                        .Excluding(s => s.FileNameWithoutExtension)
                        .Excluding(s => s.IsOwnedItem)
                        .Excluding(s => s.LocationType)
                        .Excluding(s => s.SupportsLocalMetadata)
                        .Excluding(s => s.PhysicalLocations));
        }


    }
}