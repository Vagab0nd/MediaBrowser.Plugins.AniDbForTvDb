using LanguageExt;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.PropertyMapping;
using MediaBrowser.Plugins.AniMetadata.Providers;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    internal class AniDbEpisodeMetadataFactory : IEpisodeMetadataFactory
    {
        private readonly IPropertyMappingCollection _propertyMappingCollection;

        public AniDbEpisodeMetadataFactory(IPluginConfiguration configuration)
        {
            _propertyMappingCollection = configuration.GetEpisodeMetadataMapping();
        }

        public MetadataResult<Episode> NullResult => new MetadataResult<Episode>();

        public MetadataResult<Episode> CreateMetadata(AniDbEpisodeData aniDbEpisodeData,
            MappedEpisodeResult mappedEpisodeResult)
        {
            var metadata =
                _propertyMappingCollection.Apply(aniDbEpisodeData, new MetadataResult<Episode> { Item = new Episode() })
                    .Apply(m => SetIndexes(m, mappedEpisodeResult, aniDbEpisodeData));

            if (string.IsNullOrWhiteSpace(metadata.Item.Name))
            {
                metadata = NullResult;
            }

            return metadata;
        }

        public MetadataResult<Episode> CreateMetadata(AniDbEpisodeData aniDbEpisodeData,
            TvDbEpisodeData tvDbEpisodeData, MappedEpisodeResult mappedEpisodeResult)
        {
            var metadata =
                _propertyMappingCollection.Apply(new object[] { aniDbEpisodeData, tvDbEpisodeData },
                        new MetadataResult<Episode> { Item = new Episode() })
                    .Apply(m => SetIndexes(m, mappedEpisodeResult, aniDbEpisodeData));

            if (string.IsNullOrWhiteSpace(metadata.Item.Name))
            {
                metadata = NullResult;
            }

            return metadata;
        }

        private MetadataResult<Episode> SetIndexes(MetadataResult<Episode> metadata,
            MappedEpisodeResult mappedEpisodeResult, AniDbEpisodeData aniDbEpisodeData)
        {
            var episode = metadata.Item;

            episode.ProviderIds.Add(ProviderNames.AniDb, aniDbEpisodeData.Id.ToString());

            mappedEpisodeResult.Switch(tvDbEpisodeNumber =>
                {
                    episode.IndexNumber = tvDbEpisodeNumber.EpisodeIndex;
                    episode.ParentIndexNumber = tvDbEpisodeNumber.SeasonIndex;

                    tvDbEpisodeNumber.TvDbEpisodeId.Iter(id =>
                        episode.SetProviderId(MetadataProviders.Tvdb, id.ToString()));

                    tvDbEpisodeNumber.FollowingTvDbEpisodeNumber.Iter(followingEpisode =>
                    {
                        episode.AirsBeforeSeasonNumber = followingEpisode.SeasonIndex;
                        episode.AirsBeforeEpisodeNumber = followingEpisode.EpisodeIndex;
                    });
                },
                absoluteEpisodeNumber =>
                {
                    episode.AbsoluteEpisodeNumber = absoluteEpisodeNumber.EpisodeIndex;

                    absoluteEpisodeNumber.TvDbEpisodeId.Iter(id =>
                        episode.SetProviderId(MetadataProviders.Tvdb, id.ToString()));
                },
                unknownEpisodeNumber => episode.IndexNumber = aniDbEpisodeData.EpisodeNumber.Number);

            return metadata;
        }
    }
}