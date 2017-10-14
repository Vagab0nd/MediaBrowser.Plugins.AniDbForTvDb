using System;
using LanguageExt;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
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

        public MetadataResult<Episode> CreateMetadata(EpisodeData episodeData, string metadataLanguage)
        {
            return episodeData.Match(aniDbOnly => CreateMetadata(aniDbOnly.EpisodeData, metadataLanguage),
                combined => CreateMetadata(combined.AniDbEpisodeData, combined.TvDbEpisodeData,
                    combined.FollowingTvDbEpisodeData, metadataLanguage),
                noData => NullResult);
        }

        private MetadataResult<Episode> CreateMetadata(AniDbEpisodeData aniDbEpisodeData, string metadataLanguage)
        {
            var metadata =
                _configuration.GetEpisodeMetadataMapping(metadataLanguage)
                    .Apply(aniDbEpisodeData,
                        new MetadataResult<Episode> { HasMetadata = true, Item = new Episode() }, m => _log.Debug(m))
                    .Apply(m => SetIndexes(m, aniDbEpisodeData, Option<TvDbEpisodeData>.None, new NoEpisodeData()));

            if (string.IsNullOrWhiteSpace(metadata.Item.Name))
            {
                _log.Debug("Name field not mapped, cannot continue");
                metadata = NullResult;
            }

            return metadata;
        }

        private MetadataResult<Episode> CreateMetadata(AniDbEpisodeData aniDbEpisodeData,
            TvDbEpisodeData tvDbEpisodeData, EpisodeData followingTvDbEpisodeData, string metadataLanguage)
        {
            var metadata =
                _configuration.GetEpisodeMetadataMapping(metadataLanguage)
                    .Apply(new object[] { aniDbEpisodeData, tvDbEpisodeData },
                        new MetadataResult<Episode> { HasMetadata = true, Item = new Episode() }, m => _log.Debug(m))
                    .Apply(m => SetIndexes(m, aniDbEpisodeData, tvDbEpisodeData, followingTvDbEpisodeData));

            if (string.IsNullOrWhiteSpace(metadata.Item.Name))
            {
                metadata = NullResult;
            }

            return metadata;
        }

        private MetadataResult<Episode> SetIndexes(MetadataResult<Episode> metadata, AniDbEpisodeData aniDbEpisodeData,
            Option<TvDbEpisodeData> tvDbEpisodeData,
            EpisodeData followingTvDbEpisodeData)
        {
            switch (_libraryStructure)
            {
                case LibraryStructure.AniDb:
                    return SetAniDbStructureIndexes(metadata, aniDbEpisodeData, tvDbEpisodeData);

                case LibraryStructure.TvDb:
                    return SetTvDbStructureIndexes(metadata, aniDbEpisodeData, tvDbEpisodeData,
                        followingTvDbEpisodeData);

                default:
                    throw new ArgumentOutOfRangeException(nameof(_libraryStructure));
            }
        }

        private MetadataResult<Episode> SetAniDbStructureIndexes(MetadataResult<Episode> metadata,
            AniDbEpisodeData aniDbEpisodeData, Option<TvDbEpisodeData> tvDbEpisodeData)
        {
            _log.Debug("Setting Ids for AniDb library structure");

            var episode = metadata.Item;

            episode.ProviderIds.Add(ProviderNames.AniDb, aniDbEpisodeData.Id.ToString());
            tvDbEpisodeData.Iter(d => episode.SetProviderId(MetadataProviders.Tvdb, d.Id.ToString()));

            if (aniDbEpisodeData.EpisodeNumber.Type == EpisodeType.Normal)
            {
                episode.AbsoluteEpisodeNumber = aniDbEpisodeData.EpisodeNumber.Number;
            }

            episode.IndexNumber = aniDbEpisodeData.EpisodeNumber.Number;
            episode.ParentIndexNumber = aniDbEpisodeData.EpisodeNumber.SeasonNumber;
            
            return metadata;
        }

        private MetadataResult<Episode> SetTvDbStructureIndexes(MetadataResult<Episode> metadata,
            AniDbEpisodeData aniDbEpisodeData, Option<TvDbEpisodeData> tvDbEpisodeData,
            EpisodeData followingTvDbEpisodeData)
        {
            _log.Debug("Setting Ids for TvDb library structure");

            var episode = metadata.Item;

            episode.ProviderIds.Add(ProviderNames.AniDb, aniDbEpisodeData.Id.ToString());
            tvDbEpisodeData.Iter(d =>
            {
                episode.SetProviderId(MetadataProviders.Tvdb, d.Id.ToString());

                episode.IndexNumber = d.AiredEpisodeNumber;
                episode.ParentIndexNumber = d.AiredSeason;
                d.AbsoluteNumber.IfSome(a => episode.AbsoluteEpisodeNumber = (int)a);
            });

            followingTvDbEpisodeData.Switch(aniDbOnly => { },
                combined =>
                {
                    episode.AirsBeforeSeasonNumber = combined.TvDbEpisodeData.AiredSeason;
                    episode.AirsBeforeEpisodeNumber = combined.TvDbEpisodeData.AiredEpisodeNumber;
                },
                noData => { });

            return metadata;
        }
    }
}