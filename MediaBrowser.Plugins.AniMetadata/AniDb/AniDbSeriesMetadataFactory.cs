using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.Providers;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    internal class AniDbSeriesMetadataFactory : ISeriesMetadataFactory
    {
        private readonly PluginConfiguration _configuration;

        private readonly Dictionary<string, string> _creatorTypeMappings = new Dictionary<string, string>
        {
            { "Direction", PersonType.Director },
            { "Music", PersonType.Composer }
        };

        private readonly ITitleSelector _titleSelector;

        public AniDbSeriesMetadataFactory(ITitleSelector titleSelector, PluginConfiguration configuration)
        {
            _titleSelector = titleSelector;
            _configuration = configuration;
        }

        public MetadataResult<Series> NullSeriesResult => new MetadataResult<Series>();

        public MetadataResult<Series> CreateMetadata(AniDbSeriesData aniDbSeriesData,
            string metadataLanguage)
        {
            var selectedTitle = _titleSelector.SelectTitle(aniDbSeriesData.Titles, _configuration.TitlePreference,
                metadataLanguage);

            var metadataResult = NullSeriesResult;

            selectedTitle.Match(t => metadataResult = new MetadataResult<Series>
                {
                    HasMetadata = true,
                    Item = CreateEmbySeries(aniDbSeriesData, t.Title),
                    People = GetPeople(aniDbSeriesData).ToList()
                },
                () => { });

            return metadataResult;
        }

        private Series CreateEmbySeries(AniDbSeriesData aniDbSeriesData, string selectedTitle)
        {
            var embySeries = new Series
            {
                PremiereDate = aniDbSeriesData.StartDate,
                EndDate = aniDbSeriesData.EndDate,
                Name = selectedTitle,
                Overview = ReplaceLineFeedWithNewLine(RemoveAniDbLinks(aniDbSeriesData.Description)),
                CommunityRating = aniDbSeriesData.Ratings.OfType<PermanentRatingData>().Single().Value
            };

            embySeries.ProviderIds.Add(ProviderNames.AniDb, aniDbSeriesData.Id.ToString());
            embySeries.Studios = embySeries.Studios.Concat(GetStudios(aniDbSeriesData)).ToArray();

            SetGenresAndTags(embySeries, GetGenres(aniDbSeriesData));

            return embySeries;
        }

        private IEnumerable<PersonInfo> GetPeople(AniDbSeriesData aniDbSeriesData)
        {
            var characters = aniDbSeriesData.Characters.Where(c => c.Seiyuu != null)
                .Select(c => new PersonInfo
                {
                    Name = ReverseName(c.Seiyuu.Name),
                    ImageUrl = c.Seiyuu?.PictureUrl,
                    Type = PersonType.Actor,
                    Role = c.Name
                })
                .ToList();

            var creators = aniDbSeriesData.Creators.Select(c =>
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
                tags = new[] { new TagData
                {
                    Name="Anime",
                    Weight = int.MaxValue
                } }.Concat(tags);
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