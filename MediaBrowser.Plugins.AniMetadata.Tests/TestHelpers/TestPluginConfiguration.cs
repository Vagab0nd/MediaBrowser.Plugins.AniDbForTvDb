using MediaBrowser.Plugins.AniMetadata.AniDb;
using MediaBrowser.Plugins.AniMetadata.AniDb.MetadataMapping;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.MetadataMapping;
using MediaBrowser.Plugins.AniMetadata.TvDb.MetadataMapping;

namespace MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers
{
    public class TestPluginConfiguration : PluginConfiguration
    {
        public TestPluginConfiguration()
        {
            SetSeriesMappingsToDefault(new SeriesMetadataMappingFactory(
                new AniDbSeriesMetadataMappings(new AniDbParser()), new TvDbSeriesMetadataMappings()));
        }
    }
}