using System;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Plugins.AniMetadata.AniDb.Mapping;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.Providers;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    internal class AniDbEpisodeMetadataFactory : IEpisodeMetadataFactory
    {
        private readonly PluginConfiguration _configuration;

        private readonly ITitleSelector _titleSelector;

        public AniDbEpisodeMetadataFactory(ITitleSelector titleSelector, PluginConfiguration configuration)
        {
            _titleSelector = titleSelector;
            _configuration = configuration;
        }

        public MetadataResult<Episode> NullEpisodeResult => new MetadataResult<Episode>();

        public MetadataResult<Episode> CreateEpisodeMetadataResult(AniDbEpisodeData aniDbEpisodeData,
            MappedEpisodeResult tvDbEpisode, string metadataLanguage)
        {
            var selectedTitle = _titleSelector.SelectTitle(aniDbEpisodeData.Titles, _configuration.TitlePreference,
                metadataLanguage);

            return selectedTitle.Match(t => new MetadataResult<Episode>
                {
                    HasMetadata = true,
                    Item = CreateEmbyEpisode(aniDbEpisodeData, tvDbEpisode, t.Title)
                },
                () => NullEpisodeResult);
        }

        private Episode CreateEmbyEpisode(AniDbEpisodeData aniDbEpisodeData,
            MappedEpisodeResult tvDbEpisode, string selectedTitle)
        {
            var episode = new Episode
            {
                RunTimeTicks = new TimeSpan(0, aniDbEpisodeData.TotalMinutes, 0).Ticks,
                PremiereDate = aniDbEpisodeData.AirDate,
                CommunityRating = aniDbEpisodeData.Rating?.Rating,
                Name = selectedTitle,
                Overview = aniDbEpisodeData.Summary
            };

            episode.ProviderIds.Add(ProviderNames.AniDb, aniDbEpisodeData.Id.ToString());

            tvDbEpisode.Switch(
                tvDbEpisodeNumber =>
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

            return episode;
        }
    }
}