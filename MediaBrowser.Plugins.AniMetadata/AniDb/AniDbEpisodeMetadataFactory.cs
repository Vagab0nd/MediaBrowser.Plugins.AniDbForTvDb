using System;
using LanguageExt;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.Providers;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    internal class AniDbEpisodeMetadataFactory : IEpisodeMetadataFactory
    {
        private readonly IPluginConfiguration _configuration;
        private readonly LibraryStructure _libraryStructure;
        private readonly ILogger _log;

        public AniDbEpisodeMetadataFactory(IPluginConfiguration configuration, ILogManager logManager)
        {
            _configuration = configuration;
            _libraryStructure = configuration.LibraryStructure;
            _log = logManager.GetLogger(nameof(AniDbEpisodeMetadataFactory));
        }

        public MetadataResult<Episode> NullResult => new MetadataResult<Episode>();

        public MetadataResult<Episode> CreateMetadata(AniDbEpisodeData aniDbEpisodeData,
            MappedEpisodeResult mappedEpisodeResult, string metadataLanguage)
        {
            var metadata =
                _configuration.GetEpisodeMetadataMapping(metadataLanguage)
                    .Apply(aniDbEpisodeData,
                        new MetadataResult<Episode> { HasMetadata = true, Item = new Episode() }, m => _log.Debug(m))
                    .Apply(m => SetIndexes(m, mappedEpisodeResult, aniDbEpisodeData));

            if (string.IsNullOrWhiteSpace(metadata.Item.Name))
            {
                _log.Debug("Name field not mapped, cannot continue");
                metadata = NullResult;
            }

            return metadata;
        }

        public MetadataResult<Episode> CreateMetadata(AniDbEpisodeData aniDbEpisodeData,
            TvDbEpisodeData tvDbEpisodeData, MappedEpisodeResult mappedEpisodeResult, string metadataLanguage)
        {
            var metadata =
                _configuration.GetEpisodeMetadataMapping(metadataLanguage)
                    .Apply(new object[] { aniDbEpisodeData, tvDbEpisodeData },
                        new MetadataResult<Episode> { HasMetadata = true, Item = new Episode() }, m => _log.Debug(m))
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
            switch (_libraryStructure)
            {
                case LibraryStructure.AniDb:
                    return SetAniDbStructureIndexes(metadata, mappedEpisodeResult, aniDbEpisodeData);

                case LibraryStructure.TvDb:
                    return SetTvDbStructureIndexes(metadata, mappedEpisodeResult, aniDbEpisodeData);

                default:
                    throw new ArgumentOutOfRangeException(nameof(_libraryStructure));
            }
        }

        private MetadataResult<Episode> SetAniDbStructureIndexes(MetadataResult<Episode> metadata,
            MappedEpisodeResult mappedEpisodeResult, AniDbEpisodeData aniDbEpisodeData)
        {
            _log.Debug("Setting Ids for AniDb library structure");

            var episode = metadata.Item;

            episode.ProviderIds.Add(ProviderNames.AniDb, aniDbEpisodeData.Id.ToString());

            episode.AbsoluteEpisodeNumber = aniDbEpisodeData.EpisodeNumber.Number;
            episode.IndexNumber = aniDbEpisodeData.EpisodeNumber.Number;
            episode.ParentIndexNumber = aniDbEpisodeData.EpisodeNumber.SeasonNumber;

            mappedEpisodeResult.Switch(tvDbEpisodeNumber =>
                {
                    tvDbEpisodeNumber.TvDbEpisodeId.Iter(id =>
                        episode.SetProviderId(MetadataProviders.Tvdb, id.ToString()));
                },
                absoluteEpisodeNumber =>
                {
                    absoluteEpisodeNumber.TvDbEpisodeId.Iter(id =>
                        episode.SetProviderId(MetadataProviders.Tvdb, id.ToString()));
                },
                unknownEpisodeNumber => { });

            return metadata;
        }

        private MetadataResult<Episode> SetTvDbStructureIndexes(MetadataResult<Episode> metadata,
            MappedEpisodeResult mappedEpisodeResult, AniDbEpisodeData aniDbEpisodeData)
        {
            _log.Debug("Setting Ids for TvDb library structure");

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
                unknownEpisodeNumber =>
                {
                    episode.AbsoluteEpisodeNumber = aniDbEpisodeData.EpisodeNumber.Number;
                    episode.IndexNumber = aniDbEpisodeData.EpisodeNumber.Number;
                    episode.ParentIndexNumber = aniDbEpisodeData.EpisodeNumber.SeasonNumber;
                });

            return metadata;
        }
    }
}