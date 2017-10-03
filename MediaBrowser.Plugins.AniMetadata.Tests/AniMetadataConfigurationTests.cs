using FluentAssertions;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class AniMetadataConfigurationTests
    {
        [Test]
        public void GetEpisodeMetadataMapping_ReturnsMappings()
        {
            var aniMetadataConfiguration = new AniMetadataConfiguration(new PluginConfiguration(),
                new MappingConfiguration(new ISourceMappingConfiguration[]
                {
                    new AniDbSourceMappingConfiguration(new AniDbParser()),
                    new TvDbSourceMappingConfiguration()
                }));

            aniMetadataConfiguration.GetEpisodeMetadataMapping().Should().NotBeNullOrEmpty();
        }

        [Test]
        public void GetSeasonMetadataMapping_ReturnsMappings()
        {
            var aniMetadataConfiguration = new AniMetadataConfiguration(new PluginConfiguration(),
                new MappingConfiguration(new ISourceMappingConfiguration[]
                {
                    new AniDbSourceMappingConfiguration(new AniDbParser()),
                    new TvDbSourceMappingConfiguration()
                }));

            aniMetadataConfiguration.GetSeasonMetadataMapping().Should().NotBeNullOrEmpty();
        }

        [Test]
        public void GetSeriesMetadataMapping_ReturnsMappings()
        {
            var aniMetadataConfiguration = new AniMetadataConfiguration(new PluginConfiguration(),
                new MappingConfiguration(new ISourceMappingConfiguration[]
                {
                    new AniDbSourceMappingConfiguration(new AniDbParser()),
                    new TvDbSourceMappingConfiguration()
                }));

            aniMetadataConfiguration.GetSeriesMetadataMapping().Should().NotBeNullOrEmpty();
        }
    }
}