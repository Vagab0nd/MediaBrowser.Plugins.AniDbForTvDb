using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Functional.Maybe;
using FunctionalSharp.DiscriminatedUnions;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Plugins.Anime.AniDb.Data;
using MediaBrowser.Plugins.Anime.AniDb.Mapping;
using MediaBrowser.Plugins.Anime.Configuration;

namespace MediaBrowser.Plugins.Anime.Providers.AniDb2
{
    internal class EmbyMetadataFactory : IEmbyMetadataFactory
    {
        private readonly PluginConfiguration _configuration;

        private readonly Dictionary<string, string> _creatorTypeMappings = new Dictionary<string, string>
        {
            { "Direction", PersonType.Director },
            { "Music", PersonType.Composer }
        };

        private readonly ITitleSelector _titleSelector;

        public EmbyMetadataFactory(ITitleSelector titleSelector, PluginConfiguration configuration)
        {
            _titleSelector = titleSelector;
            _configuration = configuration;
        }

        public MetadataResult<Series> NullSeriesResult => new MetadataResult<Series>();

        public MetadataResult<Season> NullSeasonResult => new MetadataResult<Season>();

        public MetadataResult<Episode> NullEpisodeResult => new MetadataResult<Episode>();

        public MetadataResult<Series> CreateSeriesMetadataResult(AniDbSeries aniDbSeries, string metadataLanguage)
        {
            var selectedTitle = _titleSelector.SelectTitle(aniDbSeries.Titles, _configuration.TitlePreference,
                metadataLanguage);

            var metadataResult = NullSeriesResult;

            selectedTitle.Match(t => metadataResult = new MetadataResult<Series>
                {
                    HasMetadata = true,
                    Item = CreateEmbySeries(aniDbSeries, t.Title),
                    People = GetPeople(aniDbSeries).ToList()
                },
                () => { });

            return metadataResult;
        }

        public MetadataResult<Season> CreateSeasonMetadataResult(AniDbSeries aniDbSeries, int seasonIndex,
            string metadataLanguage)
        {
            var selectedTitle = _titleSelector.SelectTitle(aniDbSeries.Titles, _configuration.TitlePreference,
                metadataLanguage);

            var metadataResult = NullSeasonResult;

            selectedTitle.Match(t => metadataResult = new MetadataResult<Season>
                {
                    HasMetadata = true,
                    Item = CreateEmbySeason(aniDbSeries, seasonIndex, t.Title)
                },
                () => { });

            return metadataResult;
        }

        public MetadataResult<Episode> CreateEpisodeMetadataResult(AniDbEpisode aniDbEpisode,
            DiscriminatedUnion<AniDbMapper.TvDbEpisodeNumber, AniDbMapper.AbsoluteEpisodeNumber,
                AniDbMapper.UnmappedEpisodeNumber> tvDbEpisode, string metadataLanguage)
        {
            var selectedTitle = _titleSelector.SelectTitle(aniDbEpisode.Titles, _configuration.TitlePreference,
                metadataLanguage);

            var metadataResult = NullEpisodeResult;

            selectedTitle.Match(t => metadataResult = new MetadataResult<Episode>
                {
                    HasMetadata = true,
                    Item = CreateEmbyEpisode(aniDbEpisode, tvDbEpisode, t.Title)
                },
                () => { });

            return metadataResult;
        }

        private Episode CreateEmbyEpisode(AniDbEpisode aniDbEpisode,
            DiscriminatedUnion<AniDbMapper.TvDbEpisodeNumber, AniDbMapper.AbsoluteEpisodeNumber,
                AniDbMapper.UnmappedEpisodeNumber> tvDbEpisode, string selectedTitle)
        {
            var episode = new Episode
            {
                RunTimeTicks = new TimeSpan(0, aniDbEpisode.TotalMinutes, 0).Ticks,
                PremiereDate = aniDbEpisode.AirDate,
                CommunityRating = aniDbEpisode.Rating?.Rating,
                Name = selectedTitle,
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
            return aniDbSeries.Creators.Where(c => c.Type == "Animation Work").Select(c => c.Name);
        }

        private IEnumerable<string> GetGenres(AniDbSeries aniDbSeries)
        {
            var ignoredTagIds = new[] { 6, 22, 23, 60, 128, 129, 185, 216, 242, 255, 268, 269, 289 };

            var tags = aniDbSeries.Tags ?? Enumerable.Empty<Tag>();

            return tags.Where(t => t.Weight >= 400 && !ignoredTagIds.Contains(t.Id) &&
                !ignoredTagIds.Contains(t.ParentId)).OrderBy(t => t.Weight).Select(t => t.Name);
        }

        private IEnumerable<PersonInfo> GetPeople(AniDbSeries aniDbSeries)
        {
            var characters = aniDbSeries.Characters.Where(c => c.Seiyuu != null).Select(c => new PersonInfo
            {
                Name = ReverseName(c.Seiyuu.Name),
                ImageUrl = c.Seiyuu?.PictureUrl,
                Type = PersonType.Actor,
                Role = c.Name
            }).ToList();

            var creators = aniDbSeries.Creators.Select(c =>
            {
                var type = _creatorTypeMappings.ContainsKey(c.Type) ? _creatorTypeMappings[c.Type] : c.Type;

                return new PersonInfo
                {
                    Name = ReverseName(c.Name),
                    Type = type
                };
            });

            return characters.Concat(creators);
        }

        private string ReverseName(string name)
        {
            name = name ?? "";

            return string.Join(" ", name.Split(' ').Reverse());
        }

        private string RemoveAniDbLinks(string description)
        {
            if (description == null)
            {
                return "";
            }

            var aniDbUrlRegex = new Regex(@"http://anidb.net/\w+ \[(?<name>[^\]]*)\]");

            return aniDbUrlRegex.Replace(description, "${name}");
        }

        private string ReplaceLineFeedWithNewLine(string text)
        {
            return text.Replace("\n", Environment.NewLine);
        }
    }
}