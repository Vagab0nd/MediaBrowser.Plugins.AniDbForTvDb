using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FunctionalSharp.DiscriminatedUnions;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.Anime.AniDb.Data;
using MediaBrowser.Plugins.Anime.AniDb.Mapping;
using MediaBrowser.Plugins.Anime.Configuration;

namespace MediaBrowser.Plugins.Anime.Providers.AniDb2
{
    internal class EmbyMetadataFactory : IEmbyMetadataFactory
    {
        private readonly PluginConfiguration _configuration;
        private readonly ITitleSelector _titleSelector;

        public EmbyMetadataFactory(ITitleSelector titleSelector, PluginConfiguration configuration)
        {
            _titleSelector = titleSelector;
            _configuration = configuration;
        }

        public MetadataResult<Series> CreateSeriesMetadataResult(AniDbSeries aniDbSeries, string metadataLanguage)
        {
            var selectedTitle = _titleSelector.SelectTitle(aniDbSeries.Titles, _configuration.TitlePreference,
                metadataLanguage);

            var embySeries = CreateEmbySeries(aniDbSeries, selectedTitle.Title);
            var metadataResult = new MetadataResult<Series>
            {
                HasMetadata = true,
                Item = embySeries
            };

            return metadataResult;
        }

        public MetadataResult<Season> CreateSeasonMetadataResult(AniDbSeries aniDbSeries, int seasonIndex,
            string metadataLanguage)
        {
            var selectedTitle = _titleSelector.SelectTitle(aniDbSeries.Titles, _configuration.TitlePreference,
                metadataLanguage);

            var embySeason = CreateEmbySeason(aniDbSeries, seasonIndex, selectedTitle.Title);
            var metadataResult = new MetadataResult<Season>
            {
                HasMetadata = true,
                Item = embySeason
            };

            return metadataResult;
        }

        public MetadataResult<Episode> CreateEpisodeMetadataResult(AniDbEpisode aniDbEpisode,
            DiscriminatedUnion<AniDbMapper.TvDbEpisodeNumber, AniDbMapper.AbsoluteEpisodeNumber,
                AniDbMapper.UnmappedEpisodeNumber> tvDbEpisode, string metadataLanguage)
        {
            var embyEpisode = CreateEmbyEpisode(aniDbEpisode, tvDbEpisode, metadataLanguage);

            return new MetadataResult<Episode>
            {
                HasMetadata = true,
                Item = embyEpisode
            };
        }

        private Episode CreateEmbyEpisode(AniDbEpisode aniDbEpisode,
            DiscriminatedUnion<AniDbMapper.TvDbEpisodeNumber, AniDbMapper.AbsoluteEpisodeNumber,
                AniDbMapper.UnmappedEpisodeNumber> tvDbEpisode, string metadataLanguage)
        {
            var episode = new Episode
            {
                RunTimeTicks = new TimeSpan(0, aniDbEpisode.TotalMinutes, 0).Ticks,
                PremiereDate = aniDbEpisode.AirDate,
                CommunityRating = aniDbEpisode.Rating.Rating,
                Name = _titleSelector.SelectTitle(aniDbEpisode.Titles, _configuration.TitlePreference, metadataLanguage)
                    .Title,
                Overview = aniDbEpisode.Summary
            };

            episode.ProviderIds.Add(ProviderNames.AniDb, aniDbEpisode.Id.ToString());

            tvDbEpisode.Match(
                tvDbEpisodeNumber =>
                {
                    episode.IndexNumber = tvDbEpisodeNumber.EpisodeIndex;
                    episode.ParentIndexNumber = tvDbEpisodeNumber.SeasonIndex;
                },
                absoluteEpisodeNumber =>
                {
                    episode.AbsoluteEpisodeNumber = absoluteEpisodeNumber.EpisodeIndex;
                    episode.ParentIndexNumber = aniDbEpisode.EpisodeNumber.Type == EpisodeType.Special ? 0 : 1;
                },
                unknownEpisodeNumber => { episode.IndexNumber = aniDbEpisode.EpisodeNumber.Number; });

            return episode;
        }

        private Series CreateEmbySeries(AniDbSeries aniDbSeries, string selectedTitle)
        {
            var embySeries = new Series
            {
                PremiereDate = aniDbSeries.StartDate,
                EndDate = aniDbSeries.EndDate,
                Name = selectedTitle,
                Overview = ReplaceLineFeedWithNewLine(RemoveAniDbLinks(aniDbSeries.Description)),
                CommunityRating = aniDbSeries.Ratings.OfType<PermanentRating>().Single().Value
            };

            embySeries.ProviderIds.Add(ProviderNames.AniDb, aniDbSeries.Id.ToString());
            embySeries.Studios.AddRange(GetStudios(aniDbSeries));
            embySeries.Genres.AddRange(GetGenres(aniDbSeries));

            return embySeries;
        }

        private Season CreateEmbySeason(AniDbSeries aniDbSeries, int seasonIndex, string selectedTitle)
        {
            var embySeason = new Season
            {
                Name = selectedTitle,
                Overview = ReplaceLineFeedWithNewLine(RemoveAniDbLinks(aniDbSeries.Description)),
                PremiereDate = aniDbSeries.StartDate,
                EndDate = aniDbSeries.EndDate,
                CommunityRating = aniDbSeries.Ratings.OfType<PermanentRating>().Single().Value,
                IndexNumber = seasonIndex
            };

            embySeason.Studios.AddRange(GetStudios(aniDbSeries));
            embySeason.Genres.AddRange(GetGenres(aniDbSeries));

            return embySeason;
        }

        private IEnumerable<string> GetStudios(AniDbSeries aniDbSeries)
        {
            return aniDbSeries.Creators
                .Where(c => c.Type == "Animation Work")
                .Select(c => c.Name);
        }

        private IEnumerable<string> GetGenres(AniDbSeries aniDbSeries)
        {
            var ignoredTagIds = new[] { 6, 22, 23, 60, 128, 129, 185, 216, 242, 255, 268, 269, 289 };

            return aniDbSeries.Tags
                .Where(t => t.Weight >= 400 && !ignoredTagIds.Contains(t.Id) && !ignoredTagIds.Contains(t.ParentId))
                .OrderBy(t => t.Weight)
                .Select(t => t.Name);
        }

        private string RemoveAniDbLinks(string description)
        {
            var aniDbUrlRegex = new Regex(@"http://anidb.net/\w+ \[(?<name>[^\]]*)\]");

            return aniDbUrlRegex.Replace(description, "${name}");
        }

        private string ReplaceLineFeedWithNewLine(string text)
        {
            return text.Replace("\n", Environment.NewLine);
        }
    }
}