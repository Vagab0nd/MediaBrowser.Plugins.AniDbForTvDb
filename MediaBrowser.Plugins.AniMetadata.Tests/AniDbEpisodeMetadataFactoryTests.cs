using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using LanguageExt;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.PropertyMapping;
using MediaBrowser.Plugins.AniMetadata.Providers;
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

            _propertyMappingCollection.Apply(Arg.Any<object>(), Arg.Any<MetadataResult<Episode>>())
                .Returns(c => new MetadataResult<Episode> { Item = new Episode { Name = "Name" } });

            _pluginConfiguration = Substitute.For<IPluginConfiguration>();
            _pluginConfiguration.GetEpisodeMetadataMapping().Returns(_propertyMappingCollection);
        }

        private IPluginConfiguration _pluginConfiguration;
        private IPropertyMappingCollection _propertyMappingCollection;

        [Test]
        public void CreateMetadata_AbsoluteEpisodeNumber_HasTvDbEpisodeId_SetsTvDbProviderId()
        {
            var episode = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration);

            var metadata = metadataFactory.CreateMetadata(episode, new AbsoluteEpisodeNumber(531, 22));

            metadata.Item.GetProviderId(MetadataProviders.Tvdb).Should().Be("531");
        }

        [Test]
        public void CreateMetadata_AbsoluteEpisodeNumber_NoTvDbEpisodeId_DoesNotSetTvDbProviderId()
        {
            var episode = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration);

            var metadata = metadataFactory.CreateMetadata(episode, new AbsoluteEpisodeNumber(Option<int>.None, 22));

            metadata.Item.ProviderIds.Should().NotContainKey(MetadataProviders.Tvdb.ToString());
        }

        [Test]
        public void CreateMetadata_AbsoluteEpisodeNumber_SetsAbsoluteEpisodeNumber()
        {
            var episode = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration);

            var metadata = metadataFactory.CreateMetadata(episode, new AbsoluteEpisodeNumber(531, 22));

            metadata.Item.AbsoluteEpisodeNumber.Should().Be(22);
            metadata.Item.IndexNumber.Should().BeNull();
        }

        [Test]
        public void CreateMetadata_AppliesMappings()
        {
            var episode = new AniDbEpisodeData();

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration);

            metadataFactory.CreateMetadata(episode,
                new TvDbEpisodeNumber(Option<int>.None, 1, 1,
                    new TvDbEpisodeNumber(Option<int>.None, 2, 5, Option<TvDbEpisodeNumber>.None)));

            _propertyMappingCollection.Received(1).Apply(episode, Arg.Is<MetadataResult<Episode>>(m => m.Item != null));
        }

        [Test]
        public void CreateMetadata_CombinedData_AbsoluteEpisodeNumber_HasTvDbEpisodeId_SetsTvDbProviderId()
        {
            var aniDbEpisodeData = new AniDbEpisodeData();
            var tvDbEpisodeData = new TvDbEpisodeData(3, "", 12, 12, 12, 12, new DateTime(), "");

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration);

            var metadata =
                metadataFactory.CreateMetadata(aniDbEpisodeData, tvDbEpisodeData, new AbsoluteEpisodeNumber(531, 22));

            metadata.Item.GetProviderId(MetadataProviders.Tvdb).Should().Be("531");
        }

        [Test]
        public void CreateMetadata_CombinedData_AbsoluteEpisodeNumber_NoTvDbEpisodeId_DoesNotSetTvDbProviderId()
        {
            var aniDbEpisodeData = new AniDbEpisodeData();
            var tvDbEpisodeData = new TvDbEpisodeData(3, "", 12, 12, 12, 12, new DateTime(), "");

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration);

            var metadata =
                metadataFactory.CreateMetadata(aniDbEpisodeData, tvDbEpisodeData, new AbsoluteEpisodeNumber(Option<int>.None, 22));

            metadata.Item.ProviderIds.Should().NotContainKey(MetadataProviders.Tvdb.ToString());
        }

        [Test]
        public void CreateMetadata_CombinedData_AbsoluteEpisodeNumber_SetsAbsoluteEpisodeNumber()
        {
            var aniDbEpisodeData = new AniDbEpisodeData();
            var tvDbEpisodeData = new TvDbEpisodeData(3, "", 12, 12, 12, 12, new DateTime(), "");

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration);

            var metadata =
                metadataFactory.CreateMetadata(aniDbEpisodeData, tvDbEpisodeData, new AbsoluteEpisodeNumber(531, 22));

            metadata.Item.AbsoluteEpisodeNumber.Should().Be(22);
            metadata.Item.IndexNumber.Should().BeNull();
        }

        [Test]
        public void CreateMetadata_CombinedData_AppliesMappings()
        {
            var aniDbEpisodeData = new AniDbEpisodeData();
            var tvDbEpisodeData = new TvDbEpisodeData(3, "", 12, 12, 12, 12, new DateTime(), "");

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration);

            metadataFactory.CreateMetadata(aniDbEpisodeData, tvDbEpisodeData,
                new TvDbEpisodeNumber(Option<int>.None, 1, 1,
                    new TvDbEpisodeNumber(Option<int>.None, 2, 5, Option<TvDbEpisodeNumber>.None)));

            _propertyMappingCollection.Received(1)
                .Apply(
                    Arg.Is<IEnumerable<object>>(
                        e => e.SequenceEqual(new object[] { aniDbEpisodeData, tvDbEpisodeData })),
                    Arg.Is<MetadataResult<Episode>>(m => m.Item != null));
        }

        [Test]
        public void CreateMetadata_CombinedData_MappedEpisodeNumber_SetsIndexFieldsToTvDbIndexes()
        {
            var aniDbEpisodeData = new AniDbEpisodeData();
            var tvDbEpisodeData = new TvDbEpisodeData(3, "", 12, 12, 12, 12, new DateTime(), "");

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration);

            var metadata = metadataFactory.CreateMetadata(aniDbEpisodeData, tvDbEpisodeData,
                new TvDbEpisodeNumber(523, 3, 44, Option<TvDbEpisodeNumber>.None));

            metadata.Item.IndexNumber.Should().Be(44);
            metadata.Item.ParentIndexNumber.Should().Be(3);
            metadata.Item.GetProviderId(MetadataProviders.Tvdb).Should().Be("523");
        }

        [Test]
        public void CreateMetadata_CombinedData_MappedEpisodeNumberWithNoEpisodeId_DoesNotSetTvDbProviderId()
        {
            var aniDbEpisodeData = new AniDbEpisodeData();
            var tvDbEpisodeData = new TvDbEpisodeData(3, "", 12, 12, 12, 12, new DateTime(), "");

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration);

            var metadata = metadataFactory.CreateMetadata(aniDbEpisodeData, tvDbEpisodeData,
                new TvDbEpisodeNumber(Option<int>.None, 3, 44, Option<TvDbEpisodeNumber>.None));

            metadata.Item.IndexNumber.Should().Be(44);
            metadata.Item.ParentIndexNumber.Should().Be(3);
            metadata.Item.ProviderIds.Should().NotContainKey(MetadataProviders.Tvdb.ToString());
        }

        [Test]
        public void CreateMetadata_CombinedData_NameNotSet_ReturnsNullResult()
        {
            var aniDbEpisodeData = new AniDbEpisodeData();
            var tvDbEpisodeData = new TvDbEpisodeData(3, "", 12, 12, 12, 12, new DateTime(), "");

            _propertyMappingCollection.Apply(Arg.Any<object>(), Arg.Any<MetadataResult<Episode>>())
                .Returns(c => new MetadataResult<Episode> { Item = new Episode() });

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration);

            metadataFactory.CreateMetadata(aniDbEpisodeData, tvDbEpisodeData,
                    new TvDbEpisodeNumber(Option<int>.None, 1, 1,
                        new TvDbEpisodeNumber(Option<int>.None, 2, 5, Option<TvDbEpisodeNumber>.None)))
                .ShouldBeEquivalentTo(metadataFactory.NullResult);
        }

        [Test]
        public void CreateMetadata_CombinedData_SetsAniDbProviderId()
        {
            var aniDbEpisodeData = new AniDbEpisodeData
            {
                Id = 43
            };
            var tvDbEpisodeData = new TvDbEpisodeData(3, "", 12, 12, 12, 12, new DateTime(), "");

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration);

            metadataFactory.CreateMetadata(aniDbEpisodeData, tvDbEpisodeData,
                    new TvDbEpisodeNumber(Option<int>.None, 1, 1,
                        new TvDbEpisodeNumber(Option<int>.None, 2, 5, Option<TvDbEpisodeNumber>.None)))
                .Item.ProviderIds[ProviderNames.AniDb]
                .Should()
                .Be("43");
        }

        [Test]
        public void CreateMetadata_CombinedData_UnmappedEpisodeNumber_UsesAniDbEpisodeNumber()
        {
            var aniDbEpisodeData = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };
            var tvDbEpisodeData = new TvDbEpisodeData(3, "", 12, 12, 12, 12, new DateTime(), "");

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration);

            metadataFactory.CreateMetadata(aniDbEpisodeData, tvDbEpisodeData, new UnmappedEpisodeNumber())
                .Item.IndexNumber.Should()
                .Be(64);
        }

        [Test]
        public void CreateMetadata_MappedEpisodeNumber_HasFollowingEpisode_SetAirsBeforeFields()
        {
            var episode = new AniDbEpisodeData();

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration);

            var result = metadataFactory.CreateMetadata(episode,
                new TvDbEpisodeNumber(Option<int>.None, 1, 1,
                    new TvDbEpisodeNumber(Option<int>.None, 2, 5, Option<TvDbEpisodeNumber>.None)));

            result.Item.AirsBeforeSeasonNumber.Should().Be(2);
            result.Item.AirsBeforeEpisodeNumber.Should().Be(5);
        }

        [Test]
        public void CreateMetadata_MappedEpisodeNumber_NoFollowingEpisode_DoesNotSetAirsBeforeFields()
        {
            var episode = new AniDbEpisodeData();

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration);

            var result = metadataFactory.CreateMetadata(episode,
                new TvDbEpisodeNumber(Option<int>.None, 1, 1,
                    Option<TvDbEpisodeNumber>.None));

            result.Item.AirsBeforeSeasonNumber.Should().BeNull();
            result.Item.AirsBeforeEpisodeNumber.Should().BeNull();
        }

        [Test]
        public void CreateMetadata_MappedEpisodeNumber_SetsIndexFieldsToTvDbIndexes()
        {
            var episode = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration);

            var metadata = metadataFactory.CreateMetadata(episode,
                new TvDbEpisodeNumber(523, 3, 44, Option<TvDbEpisodeNumber>.None));

            metadata.Item.IndexNumber.Should().Be(44);
            metadata.Item.ParentIndexNumber.Should().Be(3);
            metadata.Item.GetProviderId(MetadataProviders.Tvdb).Should().Be("523");
        }

        [Test]
        public void CreateMetadata_MappedEpisodeNumberWithNoEpisodeId_DoesNotSetTvDbProviderId()
        {
            var episode = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration);

            var metadata = metadataFactory.CreateMetadata(episode,
                new TvDbEpisodeNumber(Option<int>.None, 3, 44, Option<TvDbEpisodeNumber>.None));

            metadata.Item.IndexNumber.Should().Be(44);
            metadata.Item.ParentIndexNumber.Should().Be(3);
            metadata.Item.ProviderIds.Should().NotContainKey(MetadataProviders.Tvdb.ToString());
        }

        [Test]
        public void CreateMetadata_NameNotSet_ReturnsNullResult()
        {
            var episode = new AniDbEpisodeData();

            _propertyMappingCollection.Apply(Arg.Any<object>(), Arg.Any<MetadataResult<Episode>>())
                .Returns(c => new MetadataResult<Episode> { Item = new Episode() });

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration);

            metadataFactory.CreateMetadata(episode,
                    new TvDbEpisodeNumber(Option<int>.None, 1, 1,
                        new TvDbEpisodeNumber(Option<int>.None, 2, 5, Option<TvDbEpisodeNumber>.None)))
                .ShouldBeEquivalentTo(metadataFactory.NullResult);
        }

        [Test]
        public void CreateMetadata_SetsAniDbProviderId()
        {
            var episode = new AniDbEpisodeData
            {
                Id = 43
            };

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration);

            metadataFactory.CreateMetadata(episode,
                    new TvDbEpisodeNumber(Option<int>.None, 1, 1,
                        new TvDbEpisodeNumber(Option<int>.None, 2, 5, Option<TvDbEpisodeNumber>.None)))
                .Item.ProviderIds[ProviderNames.AniDb]
                .Should()
                .Be("43");
        }

        [Test]
        public void CreateMetadata_UnmappedEpisodeNumber_UsesAniDbEpisodeNumber()
        {
            var episode = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "64",
                    RawType = 1
                }
            };

            var metadataFactory = new AniDbEpisodeMetadataFactory(_pluginConfiguration);

            metadataFactory.CreateMetadata(episode, new UnmappedEpisodeNumber()).Item.IndexNumber.Should().Be(64);
        }
    }
}