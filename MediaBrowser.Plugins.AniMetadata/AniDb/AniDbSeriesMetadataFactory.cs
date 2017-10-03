using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    internal class AniDbSeriesMetadataFactory : ISeriesMetadataFactory
    {
        private readonly IPluginConfiguration _configuration;

        public AniDbSeriesMetadataFactory(IPluginConfiguration configuration)
        {
            _configuration = configuration;
        }

        public MetadataResult<Series> NullResult => new MetadataResult<Series>();

        public MetadataResult<Series> CreateMetadata(AniDbSeriesData aniDbSeriesData, string metadataLanguage)
        {
            var metadata =
                _configuration.GetSeriesMetadataMapping(metadataLanguage)
                    .Apply(aniDbSeriesData,
                        new MetadataResult<Series> { HasMetadata = true, Item = new Series() });

            if (string.IsNullOrWhiteSpace(metadata.Item.Name))
            {
                metadata = NullResult;
            }

            return metadata;
        }

        public MetadataResult<Series> CreateMetadata(AniDbSeriesData aniDbSeriesData, TvDbSeriesData tvDbSeriesData,
            string metadataLanguage)
        {
            var metadata = _configuration.GetSeriesMetadataMapping(metadataLanguage)
                .Apply(
                    new object[] { aniDbSeriesData, tvDbSeriesData },
                    new MetadataResult<Series> { HasMetadata = true, Item = new Series() });

            if (string.IsNullOrWhiteSpace(metadata.Item.Name))
            {
                metadata = NullResult;
            }

            return metadata;
        }
    }
}