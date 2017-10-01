using LanguageExt;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.PropertyMapping;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    internal class AniDbSeasonMetadataFactory : ISeasonMetadataFactory
    {
        private readonly IPropertyMappingCollection _propertyMappingCollection;

        public AniDbSeasonMetadataFactory(IPluginConfiguration configuration)
        {
            _propertyMappingCollection = configuration.GetSeasonMetadataMapping();
        }

        public MetadataResult<Season> NullResult => new MetadataResult<Season>();

        public MetadataResult<Season> CreateMetadata(AniDbSeriesData aniDbSeriesData, int seasonIndex)
        {
            var metadata =
                _propertyMappingCollection.Apply(aniDbSeriesData, new MetadataResult<Season> { Item = new Season() })
                    .Apply(m => SetIndex(m, seasonIndex));

            if (string.IsNullOrWhiteSpace(metadata.Item.Name))
            {
                metadata = NullResult;
            }

            return metadata;
        }

        public MetadataResult<Season> CreateMetadata(AniDbSeriesData aniDbSeriesData, TvDbSeriesData tvDbSeriesData,
            int seasonIndex)
        {
            var metadata = _propertyMappingCollection.Apply(new object[] { aniDbSeriesData, tvDbSeriesData },
                    new MetadataResult<Season> { Item = new Season() })
                .Apply(m => SetIndex(m, seasonIndex));

            if (string.IsNullOrWhiteSpace(metadata.Item.Name))
            {
                metadata = NullResult;
            }

            return metadata;
        }

        private MetadataResult<Season> SetIndex(MetadataResult<Season> metadata, int seasonIndex)
        {
            metadata.Item.IndexNumber = seasonIndex;

            return metadata;
        }
    }
}