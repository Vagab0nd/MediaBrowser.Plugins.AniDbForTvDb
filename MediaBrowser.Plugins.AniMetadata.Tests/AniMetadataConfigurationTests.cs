using FluentAssertions;
using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using NSubstitute;
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
                    new AniDbSourceMappingConfiguration(new AniDbParser(), Substitute.For<ITitleSelector>()),
                    new TvDbSourceMappingConfiguration()
                }));

            aniMetadataConfiguration.GetEpisodeMetadataMapping("en").Should().NotBeNullOrEmpty();
        }

        [Test]
        public void GetSeasonMetadataMapping_ReturnsMappings()
        {
            var aniMetadataConfiguration = new AniMetadataConfiguration(new PluginConfiguration(),
                new MappingConfiguration(new ISourceMappingConfiguration[]
                {
                    new AniDbSourceMappingConfiguration(new AniDbParser(), Substitute.For<ITitleSelector>()),
                    new TvDbSourceMappingConfiguration()
                }));

            aniMetadataConfiguration.GetSeasonMetadataMapping("en").Should().NotBeNullOrEmpty();
        }

        [Test]
        public void GetSeriesMetadataMapping_ReturnsMappings()
        {
            var aniMetadataConfiguration = new AniMetadataConfiguration(new PluginConfiguration(),
                new MappingConfiguration(new ISourceMappingConfiguration[]
                {
                    new AniDbSourceMappingConfiguration(new AniDbParser(), Substitute.For<ITitleSelector>()),
                    new TvDbSourceMappingConfiguration()
                }));

            aniMetadataConfiguration.GetSeriesMetadataMapping("en").Should().NotBeNullOrEmpty();
        }
    }
}