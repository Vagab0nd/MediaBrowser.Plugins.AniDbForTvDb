using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LanguageExt;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.PropertyMapping;
using MediaBrowser.Plugins.AniMetadata.Providers;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;
using NSubstitute;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class AniDbEpisodeMetadataFactoryTests
    {
        [SetUp]
        public void Setup()
        {
            _propertyMappingCollection = Substitute.For<IPropertyMappingCollection>();

            _propertyMappingCollection.Apply(Arg.Any<object>(),
                    Arg.Is<MetadataResult<Episode>>(r => r.HasMetadata && r.Item != null), Arg.Any<Action<string>>())
                .Returns(c => new MetadataResult<Episode> { HasMetadata = true, Item = new Episode { Name = "Name" } });

            _pluginConfiguration = Substitute.For<IPluginConfiguration>();
            _pluginConfiguration.GetEpisodeMetadataMapping("en").Returns(_propertyMappingCollection);

            _logManager = new ConsoleLogManager();
        }

        private IPluginConfiguration _pluginConfiguration;
        private IPropertyMappingCollection _propertyMappingCollection;
        private ILogManager _logManager;

        [Test]
        [TestCase("64", 1, 64, 64, 1)]
        [TestCase("64", 2, null, 64, 0)]
        public void CreateMetadata_AniDbLibraryStructure_HasTvDbEpisode_SetsIndexesToAniDbEpisodeIndexes(
            string rawNumber, int rawType, int? expectedAbsoluteEpisodeNumber, int? expectedIndexNumber,
            int? expectedParentIndexNumber)
        {
            _pluginConfiguration.LibraryStructure.Returns(LibraryStructure.AniDb);

            var aniDbEpisodeData = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = rawNumber,
                    RawType = rawType
                }
            };

            var tvDbEpisodeData =
                new TvDbEpisodeData(531, "", 544, 22, 3, 0, new DateTime(2017, 1, 2), "", 0, 0);

            var episodeData = new CombinedEpisodeData(aniDbEpisodeData, tvDbEpisodeData, new NoEpisodeData());

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            var metadata = metadataFactory.CreateMetadata(episodeData, "en");

            metadata.Item.AbsoluteEpisodeNumber.Should().Be(expectedAbsoluteEpisodeNumber);
            metadata.Item.IndexNumber.Should().Be(expectedIndexNumber);
            metadata.Item.ParentIndexNumber.Should().Be(expectedParentIndexNumber);
        }

        [Test]
        public void CreateMetadata_AniDbLibraryStructure_NoTvDbEpisode_SetsAbsoluteEpisodeNumberToAniDbEpisodeIndex()
        {
            _pluginConfiguration.LibraryStructure.Returns(LibraryStructure.AniDb);

            var aniDbEpisodeData = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var episodeData = new AniDbOnlyEpisodeData(aniDbEpisodeData);

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            var metadata = metadataFactory.CreateMetadata(episodeData, "en");

            metadata.Item.AbsoluteEpisodeNumber.Should().Be(64);
            metadata.Item.IndexNumber.Should().Be(64);
        }

        [Test]
        public void CreateMetadata_HasTvDbEpisode_AppliesMappings()
        {
            var aniDbEpisodeData = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var tvDbEpisodeData =
                new TvDbEpisodeData(531, "", 544, 22, 3, 0, new DateTime(2017, 1, 2), "", 0, 0);

            var episodeData = new CombinedEpisodeData(aniDbEpisodeData, tvDbEpisodeData, new NoEpisodeData());

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            metadataFactory.CreateMetadata(episodeData, "en");

            _propertyMappingCollection.Received(1)
                .Apply(
                    Arg.Is<IEnumerable<object>>(
                        a => a.SequenceEqual(new object[] { aniDbEpisodeData, tvDbEpisodeData })),
                    Arg.Is<MetadataResult<Episode>>(m => m.HasMetadata && m.Item != null),
                    Arg.Any<Action<string>>());
        }

        [Test]
        [TestCase(LibraryStructure.AniDb, null, null)]
        [TestCase(LibraryStructure.TvDb, 1, 44)]
        public void CreateMetadata_HasTvDbEpisode_HasFollowingEpisode_SetAirsBeforeFields(
            LibraryStructure libraryStructure, int? expectedAirsBeforeSeasonIndex, int? expectedAiredBeforeEpisodeIndex)
        {
            _pluginConfiguration.LibraryStructure.Returns(libraryStructure);

            var aniDbEpisodeData = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var tvDbEpisodeData =
                new TvDbEpisodeData(531, "", 544, 22, 3, 0, new DateTime(2017, 1, 2), "", 0, 0);

            var episodeData = new CombinedEpisodeData(aniDbEpisodeData, tvDbEpisodeData, new CombinedEpisodeData(
                new AniDbEpisodeData
                {
                    RawEpisodeNumber = new EpisodeNumberData
                    {
                        RawNumber = expectedAiredBeforeEpisodeIndex?.ToString(),
                        RawType = 1
                    }
                },
                new TvDbEpisodeData(333, "", Option<long>.None, 44, 1, 0, new DateTime(2017, 1, 2), "", 0, 0),
                new NoEpisodeData()));

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            var result = metadataFactory.CreateMetadata(episodeData, "en");

            result.Item.AirsBeforeSeasonNumber.Should().Be(expectedAirsBeforeSeasonIndex);
            result.Item.AirsBeforeEpisodeNumber.Should().Be(expectedAiredBeforeEpisodeIndex);
        }

        [Test]
        [TestCase(LibraryStructure.AniDb)]
        [TestCase(LibraryStructure.TvDb)]
        public void CreateMetadata_HasTvDbEpisode_NameNotSet_ReturnsNullResult(LibraryStructure libraryStructure)
        {
            _pluginConfiguration.LibraryStructure.Returns(libraryStructure);

            var aniDbEpisodeData = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var tvDbEpisodeData =
                new TvDbEpisodeData(531, "", 544, 22, 3, 0, new DateTime(2017, 1, 2), "", 0, 0);

            var episodeData = new CombinedEpisodeData(aniDbEpisodeData, tvDbEpisodeData, new NoEpisodeData());

            _propertyMappingCollection.Apply(Arg.Any<object>(), Arg.Any<MetadataResult<Episode>>(),
                    Arg.Any<Action<string>>())
                .Returns(c => new MetadataResult<Episode> { Item = new Episode() });

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            metadataFactory.CreateMetadata(episodeData, "en")
                .Should().BeEquivalentTo(metadataFactory.NullResult);
        }

        [Test]
        [TestCase(LibraryStructure.AniDb)]
        [TestCase(LibraryStructure.TvDb)]
        public void CreateMetadata_HasTvDbEpisode_NoFollowingEpisode_DoesNotSetAirsBeforeFields(
            LibraryStructure libraryStructure)
        {
            _pluginConfiguration.LibraryStructure.Returns(libraryStructure);

            var aniDbEpisodeData = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var tvDbEpisodeData =
                new TvDbEpisodeData(531, "", 544, 22, 3, 0, new DateTime(2017, 1, 2), "", 0, 0);

            var episodeData = new CombinedEpisodeData(aniDbEpisodeData, tvDbEpisodeData, new NoEpisodeData());

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            var result = metadataFactory.CreateMetadata(episodeData, "en");

            result.Item.AirsBeforeSeasonNumber.Should().BeNull();
            result.Item.AirsBeforeEpisodeNumber.Should().BeNull();
        }

        [Test]
        [TestCase(LibraryStructure.AniDb)]
        [TestCase(LibraryStructure.TvDb)]
        public void CreateMetadata_HasTvDbEpisode_SetsAniDbProviderId(LibraryStructure libraryStructure)
        {
            _pluginConfiguration.LibraryStructure.Returns(libraryStructure);

            var aniDbEpisodeData = new AniDbEpisodeData
            {
                Id = 43,
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var tvDbEpisodeData =
                new TvDbEpisodeData(531, "", 544, 22, 3, 0, new DateTime(2017, 1, 2), "", 0, 0);

            var episodeData = new CombinedEpisodeData(aniDbEpisodeData, tvDbEpisodeData, new NoEpisodeData());

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            metadataFactory.CreateMetadata(episodeData, "en")
                .Item.ProviderIds[ProviderNames.AniDb]
                .Should()
                .Be("43");
        }

        [Test]
        [TestCase(LibraryStructure.AniDb)]
        [TestCase(LibraryStructure.TvDb)]
        public void CreateMetadata_HasTvDbEpisode_SetsTvDbProviderId(LibraryStructure libraryStructure)
        {
            _pluginConfiguration.LibraryStructure.Returns(libraryStructure);

            var aniDbEpisodeData = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var tvDbEpisodeData =
                new TvDbEpisodeData(531, "", Option<long>.None, 0, 0, 0, new DateTime(2017, 1, 2), "", 0, 0);

            var episodeData = new CombinedEpisodeData(aniDbEpisodeData, tvDbEpisodeData, new NoEpisodeData());

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            var metadata = metadataFactory.CreateMetadata(episodeData, "en");

            metadata.Item.GetProviderId(MetadataProviders.Tvdb).Should().Be("531");
        }

        [Test]
        public void CreateMetadata_NoTvDbEpisode_AppliesMappings()
        {
            var aniDbEpisodeData = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            var episodeData = new AniDbOnlyEpisodeData(aniDbEpisodeData);

            metadataFactory.CreateMetadata(episodeData, "en");

            _propertyMappingCollection.Received(1)
                .Apply(aniDbEpisodeData, Arg.Is<MetadataResult<Episode>>(m => m.HasMetadata && m.Item != null),
                    Arg.Any<Action<string>>());
        }

        [Test]
        [TestCase(LibraryStructure.AniDb)]
        [TestCase(LibraryStructure.TvDb)]
        public void CreateMetadata_NoTvDbEpisode_DoesNotSetAirsBeforeFields(LibraryStructure libraryStructure)
        {
            _pluginConfiguration.LibraryStructure.Returns(libraryStructure);

            var aniDbEpisodeData = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var episodeData = new AniDbOnlyEpisodeData(aniDbEpisodeData);

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            var result = metadataFactory.CreateMetadata(episodeData, "en");

            result.Item.AirsBeforeSeasonNumber.Should().BeNull();
            result.Item.AirsBeforeEpisodeNumber.Should().BeNull();
        }

        [Test]
        [TestCase(LibraryStructure.AniDb)]
        [TestCase(LibraryStructure.TvDb)]
        public void CreateMetadata_NoTvDbEpisode_DoesNotSetTvDbProviderId(
            LibraryStructure libraryStructure)
        {
            _pluginConfiguration.LibraryStructure.Returns(libraryStructure);

            var aniDbEpisodeData = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var episodeData = new AniDbOnlyEpisodeData(aniDbEpisodeData);

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            var metadata = metadataFactory.CreateMetadata(episodeData, "en");

            metadata.Item.ProviderIds.Should().NotContainKey(MetadataProviders.Tvdb.ToString());
        }

        [Test]
        [TestCase(LibraryStructure.AniDb)]
        [TestCase(LibraryStructure.TvDb)]
        public void CreateMetadata_NoTvDbEpisode_NameNotSet_ReturnsNullResult(LibraryStructure libraryStructure)
        {
            _pluginConfiguration.LibraryStructure.Returns(libraryStructure);

            var aniDbEpisodeData = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var episodeData = new AniDbOnlyEpisodeData(aniDbEpisodeData);

            _propertyMappingCollection.Apply(Arg.Any<object>(), Arg.Any<MetadataResult<Episode>>(),
                    Arg.Any<Action<string>>())
                .Returns(c => new MetadataResult<Episode> { Item = new Episode() });

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            metadataFactory.CreateMetadata(episodeData, "en")
                .Should().BeEquivalentTo(metadataFactory.NullResult);
        }

        [Test]
        [TestCase(LibraryStructure.AniDb)]
        [TestCase(LibraryStructure.TvDb)]
        public void CreateMetadata_NoTvDbEpisode_SetsAniDbProviderId(LibraryStructure libraryStructure)
        {
            _pluginConfiguration.LibraryStructure.Returns(libraryStructure);

            var aniDbEpisodeData = new AniDbEpisodeData
            {
                Id = 43,
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var episodeData = new AniDbOnlyEpisodeData(aniDbEpisodeData);

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            metadataFactory.CreateMetadata(episodeData, "en")
                .Item.ProviderIds[ProviderNames.AniDb]
                .Should()
                .Be("43");
        }

        [Test]
        [TestCase(544L, 22, 3, 544, 22, 3)]
        [TestCase(null, 22, 3, null, 22, 3)]
        public void CreateMetadata_TvDbLibraryStructure_HasTvDbEpisode_SetsIndexesToTvDbEpisodeIndexes(
            long? absoluteEpisodeNumber, int airedEpisodeNumber, int airedSeason,
            int? expectedAbsoluteEpisodeNumber, int? expectedIndexNumber, int? expectedParentIndexNumber)
        {
            _pluginConfiguration.LibraryStructure.Returns(LibraryStructure.TvDb);

            var aniDbEpisodeData = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var tvDbEpisodeData =
                new TvDbEpisodeData(531, "", absoluteEpisodeNumber.ToOption(), airedEpisodeNumber, airedSeason, 0,
                    new DateTime(2017, 1, 2), "", 0, 0);

            var episodeData = new CombinedEpisodeData(aniDbEpisodeData, tvDbEpisodeData, new NoEpisodeData());

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            var metadata = metadataFactory.CreateMetadata(episodeData, "en");

            metadata.Item.AbsoluteEpisodeNumber.Should().Be(expectedAbsoluteEpisodeNumber);
            metadata.Item.IndexNumber.Should().Be(expectedIndexNumber);
            metadata.Item.ParentIndexNumber.Should().Be(expectedParentIndexNumber);
        }

        [Test]
        public void CreateMetadata_TvDbLibraryStructure_NoTvDbEpisode_DoesNotSetAbsoluteEpisodeNumber()
        {
            _pluginConfiguration.LibraryStructure.Returns(LibraryStructure.TvDb);

            var aniDbEpisodeData = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var episodeData = new AniDbOnlyEpisodeData(aniDbEpisodeData);

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            var metadata = metadataFactory.CreateMetadata(episodeData, "en");

            metadata.Item.AbsoluteEpisodeNumber.Should().BeNull();
            metadata.Item.IndexNumber.Should().BeNull();
        }
    }
}