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
using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping;
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
        [TestCase(LibraryStructure.AniDb)]
        [TestCase(LibraryStructure.TvDb)]
        public void CreateMetadata_AbsoluteEpisodeNumber_HasTvDbEpisodeId_SetsTvDbProviderId(
            LibraryStructure libraryStructure)
        {
            _pluginConfiguration.LibraryStructure.Returns(libraryStructure);

            var episode = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            var metadata = metadataFactory.CreateMetadata(episode, new AbsoluteEpisodeNumber(531, 22), "en");

            metadata.Item.GetProviderId(MetadataProviders.Tvdb).Should().Be("531");
        }

        [Test]
        [TestCase(LibraryStructure.AniDb)]
        [TestCase(LibraryStructure.TvDb)]
        public void CreateMetadata_AbsoluteEpisodeNumber_NoTvDbEpisodeId_DoesNotSetTvDbProviderId(
            LibraryStructure libraryStructure)
        {
            _pluginConfiguration.LibraryStructure.Returns(libraryStructure);

            var episode = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            var metadata =
                metadataFactory.CreateMetadata(episode, new AbsoluteEpisodeNumber(Option<int>.None, 22), "en");

            metadata.Item.ProviderIds.Should().NotContainKey(MetadataProviders.Tvdb.ToString());
        }

        [Test]
        [TestCase(LibraryStructure.AniDb, 64, 64)]
        [TestCase(LibraryStructure.TvDb, 22, null)]
        public void CreateMetadata_AbsoluteEpisodeNumber_SetsAbsoluteEpisodeNumber(LibraryStructure libraryStructure,
            int expectedAbsoluteIndex, int? expectedIndex)
        {
            _pluginConfiguration.LibraryStructure.Returns(libraryStructure);

            var episode = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            var metadata = metadataFactory.CreateMetadata(episode, new AbsoluteEpisodeNumber(531, 22), "en");

            metadata.Item.AbsoluteEpisodeNumber.Should().Be(expectedAbsoluteIndex);
            metadata.Item.IndexNumber.Should().Be(expectedIndex);
        }

        [Test]
        public void CreateMetadata_AppliesMappings()
        {
            var episode = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            metadataFactory.CreateMetadata(episode,
                new TvDbEpisodeNumber(Option<int>.None, 1, 1,
                    new TvDbEpisodeNumber(Option<int>.None, 2, 5, Option<TvDbEpisodeNumber>.None)), "en");

            _propertyMappingCollection.Received(1)
                .Apply(episode, Arg.Is<MetadataResult<Episode>>(m => m.HasMetadata && m.Item != null),
                    Arg.Any<Action<string>>());
        }

        [Test]
        [TestCase(LibraryStructure.AniDb)]
        [TestCase(LibraryStructure.TvDb)]
        public void CreateMetadata_CombinedData_AbsoluteEpisodeNumber_HasTvDbEpisodeId_SetsTvDbProviderId(
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
            var tvDbEpisodeData = new TvDbEpisodeData(3, "", 12, 12, 12, 12, new DateTime(), "");

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            var metadata =
                metadataFactory.CreateMetadata(aniDbEpisodeData, tvDbEpisodeData, new AbsoluteEpisodeNumber(531, 22),
                    "en");

            metadata.Item.GetProviderId(MetadataProviders.Tvdb).Should().Be("531");
        }

        [Test]
        [TestCase(LibraryStructure.AniDb)]
        [TestCase(LibraryStructure.TvDb)]
        public void CreateMetadata_CombinedData_AbsoluteEpisodeNumber_NoTvDbEpisodeId_DoesNotSetTvDbProviderId(
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
            var tvDbEpisodeData = new TvDbEpisodeData(3, "", 12, 12, 12, 12, new DateTime(), "");

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            var metadata =
                metadataFactory.CreateMetadata(aniDbEpisodeData, tvDbEpisodeData,
                    new AbsoluteEpisodeNumber(Option<int>.None, 22), "en");

            metadata.Item.ProviderIds.Should().NotContainKey(MetadataProviders.Tvdb.ToString());
        }

        [Test]
        [TestCase(LibraryStructure.AniDb, 64, 64)]
        [TestCase(LibraryStructure.TvDb, 22, null)]
        public void CreateMetadata_CombinedData_AbsoluteEpisodeNumber_SetsAbsoluteEpisodeNumber(
            LibraryStructure libraryStructure, int expectedAbsoluteIndex, int? expectedIndex)
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
            var tvDbEpisodeData = new TvDbEpisodeData(3, "", 12, 12, 12, 12, new DateTime(), "");

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            var metadata =
                metadataFactory.CreateMetadata(aniDbEpisodeData, tvDbEpisodeData, new AbsoluteEpisodeNumber(531, 22),
                    "en");

            metadata.Item.AbsoluteEpisodeNumber.Should().Be(expectedAbsoluteIndex);
            metadata.Item.IndexNumber.Should().Be(expectedIndex);
        }

        [Test]
        [TestCase(LibraryStructure.AniDb)]
        [TestCase(LibraryStructure.TvDb)]
        public void CreateMetadata_CombinedData_AppliesMappings(LibraryStructure libraryStructure)
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
            var tvDbEpisodeData = new TvDbEpisodeData(3, "", 12, 12, 12, 12, new DateTime(), "");

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            metadataFactory.CreateMetadata(aniDbEpisodeData, tvDbEpisodeData,
                new TvDbEpisodeNumber(Option<int>.None, 1, 1,
                    new TvDbEpisodeNumber(Option<int>.None, 2, 5, Option<TvDbEpisodeNumber>.None)), "en");

            _propertyMappingCollection.Received(1)
                .Apply(
                    Arg.Is<IEnumerable<object>>(
                        e => e.SequenceEqual(new object[] { aniDbEpisodeData, tvDbEpisodeData })),
                    Arg.Is<MetadataResult<Episode>>(m => m.HasMetadata && m.Item != null), Arg.Any<Action<string>>());
        }

        [Test]
        [TestCase(LibraryStructure.AniDb, 64, 1)]
        [TestCase(LibraryStructure.TvDb, 44, 3)]
        public void CreateMetadata_CombinedData_MappedEpisodeNumber_SetsIndexFields(LibraryStructure libraryStructure,
            int expectedIndex, int expectedSeasonIndex)
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
            var tvDbEpisodeData = new TvDbEpisodeData(3, "", 12, 12, 12, 12, new DateTime(), "");

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            var metadata = metadataFactory.CreateMetadata(aniDbEpisodeData, tvDbEpisodeData,
                new TvDbEpisodeNumber(523, 3, 44, Option<TvDbEpisodeNumber>.None), "en");

            metadata.Item.IndexNumber.Should().Be(expectedIndex);
            metadata.Item.ParentIndexNumber.Should().Be(expectedSeasonIndex);
            metadata.Item.GetProviderId(MetadataProviders.Tvdb).Should().Be("523");
        }

        [Test]
        [TestCase(LibraryStructure.AniDb, 64, 1)]
        [TestCase(LibraryStructure.TvDb, 44, 3)]
        public void CreateMetadata_CombinedData_MappedEpisodeNumberWithNoEpisodeId_DoesNotSetTvDbProviderId(
            LibraryStructure libraryStructure, int expectedIndex, int expectedSeasonIndex)
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
            var tvDbEpisodeData = new TvDbEpisodeData(3, "", 12, 12, 12, 12, new DateTime(), "");

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            var metadata = metadataFactory.CreateMetadata(aniDbEpisodeData, tvDbEpisodeData,
                new TvDbEpisodeNumber(Option<int>.None, 3, 44, Option<TvDbEpisodeNumber>.None), "en");

            metadata.Item.IndexNumber.Should().Be(expectedIndex);
            metadata.Item.ParentIndexNumber.Should().Be(expectedSeasonIndex);
            metadata.Item.ProviderIds.Should().NotContainKey(MetadataProviders.Tvdb.ToString());
        }

        [Test]
        [TestCase(LibraryStructure.AniDb)]
        [TestCase(LibraryStructure.TvDb)]
        public void CreateMetadata_CombinedData_NameNotSet_ReturnsNullResult(LibraryStructure libraryStructure)
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
            var tvDbEpisodeData = new TvDbEpisodeData(3, "", 12, 12, 12, 12, new DateTime(), "");

            _propertyMappingCollection.Apply(Arg.Any<object>(), Arg.Any<MetadataResult<Episode>>(),
                    Arg.Any<Action<string>>())
                .Returns(c => new MetadataResult<Episode> { Item = new Episode() });

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            metadataFactory.CreateMetadata(aniDbEpisodeData, tvDbEpisodeData,
                    new TvDbEpisodeNumber(Option<int>.None, 1, 1,
                        new TvDbEpisodeNumber(Option<int>.None, 2, 5, Option<TvDbEpisodeNumber>.None)), "en")
                .ShouldBeEquivalentTo(metadataFactory.NullResult);
        }

        [Test]
        [TestCase(LibraryStructure.AniDb)]
        [TestCase(LibraryStructure.TvDb)]
        public void CreateMetadata_CombinedData_SetsAniDbProviderId(LibraryStructure libraryStructure)
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
            var tvDbEpisodeData = new TvDbEpisodeData(3, "", 12, 12, 12, 12, new DateTime(), "");

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            metadataFactory.CreateMetadata(aniDbEpisodeData, tvDbEpisodeData,
                    new TvDbEpisodeNumber(Option<int>.None, 1, 1,
                        new TvDbEpisodeNumber(Option<int>.None, 2, 5, Option<TvDbEpisodeNumber>.None)), "en")
                .Item.ProviderIds[ProviderNames.AniDb]
                .Should()
                .Be("43");
        }

        [Test]
        [TestCase(LibraryStructure.AniDb)]
        [TestCase(LibraryStructure.TvDb)]
        public void CreateMetadata_CombinedData_UnmappedEpisodeNumber_UsesAniDbEpisodeNumber(
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
            var tvDbEpisodeData = new TvDbEpisodeData(3, "", 12, 12, 12, 12, new DateTime(), "");

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            metadataFactory.CreateMetadata(aniDbEpisodeData, tvDbEpisodeData, new UnmappedEpisodeNumber(), "en")
                .Item.IndexNumber.Should()
                .Be(64);
        }

        [Test]
        [TestCase(LibraryStructure.AniDb)]
        [TestCase(LibraryStructure.TvDb)]
        public void CreateMetadata_MappedEpisodeNumber_HasFollowingEpisode_SetAirsBeforeFields(
            LibraryStructure libraryStructure)
        {
            _pluginConfiguration.LibraryStructure.Returns(libraryStructure);

            var episode = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            var result = metadataFactory.CreateMetadata(episode,
                new TvDbEpisodeNumber(Option<int>.None, 1, 1,
                    new TvDbEpisodeNumber(Option<int>.None, 2, 5, Option<TvDbEpisodeNumber>.None)), "en");

            if (libraryStructure == LibraryStructure.TvDb)
            {
                result.Item.AirsBeforeSeasonNumber.Should().Be(2);
                result.Item.AirsBeforeEpisodeNumber.Should().Be(5);
            }
            else
            {
                result.Item.AirsBeforeSeasonNumber.Should().BeNull();
                result.Item.AirsBeforeEpisodeNumber.Should().BeNull();
            }
        }

        [Test]
        [TestCase(LibraryStructure.AniDb)]
        [TestCase(LibraryStructure.TvDb)]
        public void CreateMetadata_MappedEpisodeNumber_NoFollowingEpisode_DoesNotSetAirsBeforeFields(
            LibraryStructure libraryStructure)
        {
            _pluginConfiguration.LibraryStructure.Returns(libraryStructure);

            var episode = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            var result = metadataFactory.CreateMetadata(episode,
                new TvDbEpisodeNumber(Option<int>.None, 1, 1,
                    Option<TvDbEpisodeNumber>.None), "en");

            result.Item.AirsBeforeSeasonNumber.Should().BeNull();
            result.Item.AirsBeforeEpisodeNumber.Should().BeNull();
        }

        [Test]
        [TestCase(LibraryStructure.AniDb, 64, 1)]
        [TestCase(LibraryStructure.TvDb, 44, 3)]
        public void CreateMetadata_MappedEpisodeNumber_SetsIndexFields(LibraryStructure libraryStructure,
            int expectedIndex, int expectedSeasonIndex)
        {
            _pluginConfiguration.LibraryStructure.Returns(libraryStructure);

            var episode = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            var metadata = metadataFactory.CreateMetadata(episode,
                new TvDbEpisodeNumber(523, 3, 44, Option<TvDbEpisodeNumber>.None), "en");

            metadata.Item.IndexNumber.Should().Be(expectedIndex);
            metadata.Item.ParentIndexNumber.Should().Be(expectedSeasonIndex);
            metadata.Item.GetProviderId(MetadataProviders.Tvdb).Should().Be("523");
        }

        [Test]
        [TestCase(LibraryStructure.AniDb, 64, 1)]
        [TestCase(LibraryStructure.TvDb, 44, 3)]
        public void CreateMetadata_MappedEpisodeNumberWithNoEpisodeId_DoesNotSetTvDbProviderId(
            LibraryStructure libraryStructure, int expectedIndex, int expectedSeasonIndex)
        {
            _pluginConfiguration.LibraryStructure.Returns(libraryStructure);

            var episode = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            var metadata = metadataFactory.CreateMetadata(episode,
                new TvDbEpisodeNumber(Option<int>.None, 3, 44, Option<TvDbEpisodeNumber>.None), "en");

            metadata.Item.IndexNumber.Should().Be(expectedIndex);
            metadata.Item.ParentIndexNumber.Should().Be(expectedSeasonIndex);
            metadata.Item.ProviderIds.Should().NotContainKey(MetadataProviders.Tvdb.ToString());
        }

        [Test]
        [TestCase(LibraryStructure.AniDb)]
        [TestCase(LibraryStructure.TvDb)]
        public void CreateMetadata_NameNotSet_ReturnsNullResult(LibraryStructure libraryStructure)
        {
            _pluginConfiguration.LibraryStructure.Returns(libraryStructure);

            var episode = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            _propertyMappingCollection.Apply(Arg.Any<object>(), Arg.Any<MetadataResult<Episode>>(),
                    Arg.Any<Action<string>>())
                .Returns(c => new MetadataResult<Episode> { Item = new Episode() });

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            metadataFactory.CreateMetadata(episode,
                    new TvDbEpisodeNumber(Option<int>.None, 1, 1,
                        new TvDbEpisodeNumber(Option<int>.None, 2, 5, Option<TvDbEpisodeNumber>.None)), "en")
                .ShouldBeEquivalentTo(metadataFactory.NullResult);
        }

        [Test]
        [TestCase(LibraryStructure.AniDb)]
        [TestCase(LibraryStructure.TvDb)]
        public void CreateMetadata_SetsAniDbProviderId(LibraryStructure libraryStructure)
        {
            _pluginConfiguration.LibraryStructure.Returns(libraryStructure);

            var episode = new AniDbEpisodeData
            {
                Id = 43,
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            metadataFactory.CreateMetadata(episode,
                    new TvDbEpisodeNumber(Option<int>.None, 1, 1,
                        new TvDbEpisodeNumber(Option<int>.None, 2, 5, Option<TvDbEpisodeNumber>.None)), "en")
                .Item.ProviderIds[ProviderNames.AniDb]
                .Should()
                .Be("43");
        }

        [Test]
        [TestCase(LibraryStructure.AniDb)]
        [TestCase(LibraryStructure.TvDb)]
        public void CreateMetadata_UnmappedEpisodeNumber_UsesAniDbEpisodeNumber(LibraryStructure libraryStructure)
        {
            _pluginConfiguration.LibraryStructure.Returns(libraryStructure);

            var episode = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration, _logManager);

            metadataFactory.CreateMetadata(episode, new UnmappedEpisodeNumber(), "en").Item.IndexNumber.Should().Be(64);
        }
    }
}