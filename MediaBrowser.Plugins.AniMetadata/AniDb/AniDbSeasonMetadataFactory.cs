using LanguageExt;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    internal class AniDbSeasonMetadataFactory : ISeasonMetadataFactory
    {
        private readonly IPluginConfiguration _configuration;

        public AniDbSeasonMetadataFactory(IPluginConfiguration configuration)
        {
            _configuration = configuration;
        }

        public MetadataResult<Season> NullResult => new MetadataResult<Season>();

        public MetadataResult<Season> CreateMetadata(AniDbSeriesData aniDbSeriesData, int seasonIndex,
            string metadataLanguage)
        {
            var metadata =
                _configuration.GetSeasonMetadataMapping(metadataLanguage)
                    .Apply(aniDbSeriesData,
                        new MetadataResult<Season> { HasMetadata = true, Item = new Season() })
                    .Apply(m => SetIndex(m, seasonIndex));

            if (string.IsNullOrWhiteSpace(metadata.Item.Name))
            {
                metadata = NullResult;
            }

            return metadata;
        }

        public MetadataResult<Season> CreateMetadata(AniDbSeriesData aniDbSeriesData, TvDbSeriesData tvDbSeriesData,
            int seasonIndex, string metadataLanguage)
        {
            var metadata = _configuration.GetSeasonMetadataMapping(metadataLanguage)
                .Apply(new object[] { aniDbSeriesData, tvDbSeriesData },
                    new MetadataResult<Season> { HasMetadata = true, Item = new Season() })
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