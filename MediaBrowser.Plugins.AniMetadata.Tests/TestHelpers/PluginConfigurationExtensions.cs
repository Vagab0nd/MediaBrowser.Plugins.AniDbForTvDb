using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.MetadataMapping;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.MetadataMapping;
using MediaBrowser.Plugins.AniMetadata.TvDb.MetadataMapping;

namespace MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers
{
    public static class PluginConfigurationExtensions
    {
        public static PluginConfiguration SetDefaultSeriesMappings(this PluginConfiguration pluginConfiguration)
        {
            pluginConfiguration.SetSeriesMappingsToDefault(
                new SeriesMetadataMappingFactory(
                    new AniDbSeriesMetadataMappings(new AniDbParser()), new TvDbSeriesMetadataMappings()));

            return pluginConfiguration;
        }
    }
}