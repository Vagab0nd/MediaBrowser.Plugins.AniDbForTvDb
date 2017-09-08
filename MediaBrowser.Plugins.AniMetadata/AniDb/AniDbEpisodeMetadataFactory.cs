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

        public MetadataResult<Episode> CreateEpisodeMetadataResult(EpisodeData episodeData,
            MappedEpisodeResult tvDbEpisode, string metadataLanguage)
        {
            var selectedTitle = _titleSelector.SelectTitle(episodeData.Titles, _configuration.TitlePreference,
                metadataLanguage);

            return selectedTitle.Match(t => new MetadataResult<Episode>
                {
                    HasMetadata = true,
                    Item = CreateEmbyEpisode(episodeData, tvDbEpisode, t.Title)
                },
                () => NullEpisodeResult);
        }

        private Episode CreateEmbyEpisode(EpisodeData episodeData,
            MappedEpisodeResult tvDbEpisode, string selectedTitle)
        {
            var episode = new Episode
            {
                RunTimeTicks = new TimeSpan(0, episodeData.TotalMinutes, 0).Ticks,
                PremiereDate = episodeData.AirDate,
                CommunityRating = episodeData.Rating?.Rating,
                Name = selectedTitle,
                Overview = episodeData.Summary
            };

            episode.ProviderIds.Add(ProviderNames.AniDb, episodeData.Id.ToString());

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
                unknownEpisodeNumber => episode.IndexNumber = episodeData.EpisodeNumber.Number);

            return episode;
        }
    }
}