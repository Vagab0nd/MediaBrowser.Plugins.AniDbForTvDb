using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.PropertyMapping;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    internal class AniDbSeriesMetadataFactory : ISeriesMetadataFactory
    {
        private readonly IPropertyMappingCollection _propertyMappingCollection;

        public AniDbSeriesMetadataFactory(IPluginConfiguration configuration)
        {
            _propertyMappingCollection = configuration.GetSeriesMetadataMapping();
        }

        public MetadataResult<Series> NullResult => new MetadataResult<Series>();

        public MetadataResult<Series> CreateMetadata(AniDbSeriesData aniDbSeriesData)
        {
            var metadata =
                _propertyMappingCollection.Apply(aniDbSeriesData, new MetadataResult<Series> { Item = new Series() });

            if (string.IsNullOrWhiteSpace(metadata.Item.Name))
            {
                metadata = NullResult;
            }

            return metadata;
        }

        public MetadataResult<Series> CreateMetadata(AniDbSeriesData aniDbSeriesData, TvDbSeriesData tvDbSeriesData)
        {
            var metadata = _propertyMappingCollection.Apply(new object[] { aniDbSeriesData, tvDbSeriesData },
                new MetadataResult<Series> { Item = new Series() });

            if (string.IsNullOrWhiteSpace(metadata.Item.Name))
            {
                metadata = NullResult;
            }

            return metadata;
        }
    }
}