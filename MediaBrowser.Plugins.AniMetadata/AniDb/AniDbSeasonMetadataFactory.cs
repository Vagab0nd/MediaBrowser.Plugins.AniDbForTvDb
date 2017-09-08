using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Configuration;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    internal class AniDbSeasonMetadataFactory : ISeasonMetadataFactory
    {
        private readonly PluginConfiguration _configuration;
        private readonly ITitleSelector _titleSelector;

        public AniDbSeasonMetadataFactory(ITitleSelector titleSelector, PluginConfiguration configuration)
        {
            _titleSelector = titleSelector;
            _configuration = configuration;
        }

        public MetadataResult<Season> NullSeasonResult => new MetadataResult<Season>();

        public MetadataResult<Season> CreateMetadata(AniDbSeriesData aniDbSeriesData, int seasonIndex,
            string metadataLanguage)
        {
            var selectedTitle = _titleSelector.SelectTitle(aniDbSeriesData.Titles, _configuration.TitlePreference,
                metadataLanguage);

            var metadataResult = NullSeasonResult;

            selectedTitle.Match(t => metadataResult = new MetadataResult<Season>
                {
                    HasMetadata = true,
                    Item = CreateEmbySeason(aniDbSeriesData, seasonIndex, t.Title)
                },
                () => { });

            return metadataResult;
        }

        private Season CreateEmbySeason(AniDbSeriesData aniDbSeriesData, int seasonIndex, string selectedTitle)
        {
            var embySeason = new Season
            {
                Name = selectedTitle,
                Overview = ReplaceLineFeedWithNewLine(RemoveAniDbLinks(aniDbSeriesData.Description)),
                PremiereDate = aniDbSeriesData.StartDate,
                EndDate = aniDbSeriesData.EndDate,
                CommunityRating = aniDbSeriesData.Ratings.OfType<PermanentRatingData>().Single().Value,
                IndexNumber = seasonIndex
            };

            embySeason.Studios = embySeason.Studios.Concat(GetStudios(aniDbSeriesData)).ToArray();

            SetGenresAndTags(embySeason, GetGenres(aniDbSeriesData));

            return embySeason;
        }

        private IEnumerable<string> GetStudios(AniDbSeriesData aniDbSeriesData)
        {
            return aniDbSeriesData.Creators.Where(c => c.Type == "Animation Work").Select(c => c.Name);
        }

        private IEnumerable<string> GetGenres(AniDbSeriesData aniDbSeriesData)
        {
            var ignoredTagIds = new[] { 6, 22, 23, 60, 128, 129, 185, 216, 242, 255, 268, 269, 289 };

            var tags = aniDbSeriesData.Tags ?? Enumerable.Empty<TagData>();

            if (_configuration.AddAnimeGenre)
            {
                tags = new[]
                {
                    new TagData
                    {
                        Name = "Anime",
                        Weight = int.MaxValue
                    }
                }.Concat(tags);
            }

            return tags.Where(t => t.Weight >= 400 && !ignoredTagIds.Contains(t.Id) &&
                    !ignoredTagIds.Contains(t.ParentId))
                .OrderByDescending(t => t.Weight)
                .Select(t => t.Name);
        }

        private void SetGenresAndTags(BaseItem item, IEnumerable<string> genres)
        {
            var maxGenres = _configuration.MaxGenres > 0 ? _configuration.MaxGenres : int.MaxValue;

            item.Genres.AddRange(genres.Take(maxGenres));

            if (_configuration.MoveExcessGenresToTags)
            {
                item.Tags = genres.Skip(maxGenres).ToArray();
            }
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