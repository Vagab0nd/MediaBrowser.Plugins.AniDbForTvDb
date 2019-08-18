using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    internal class AniDbParser : IAniDbParser
    {
        private readonly Dictionary<string, PersonType> _creatorTypeMappings = new Dictionary<string, PersonType>
        {
            { "Direction", PersonType.Director },
            { "Music", PersonType.Composer }
        };

        public IEnumerable<string> GetStudios(AniDbSeriesData aniDbSeriesData)
        {
            if (aniDbSeriesData.Creators == null)
            {
                return new List<string>();
            }

            return aniDbSeriesData.Creators.Where(c => c.Type == "Animation Work").Select(c => c.Name);
        }

        public IEnumerable<string> GetGenres(AniDbSeriesData aniDbSeriesData, int maxGenres, bool addAnimeGenre)
        {
            return GetGenreTags(aniDbSeriesData.Tags ?? Enumerable.Empty<TagData>(), addAnimeGenre).Take(maxGenres);
        }

        public IEnumerable<string> GetTags(AniDbSeriesData aniDbSeriesData, int maxGenres, bool addAnimeGenre)
        {
            return GetGenreTags(aniDbSeriesData.Tags ?? Enumerable.Empty<TagData>(), addAnimeGenre).Skip(maxGenres);
        }

        public string FormatDescription(string description)
        {
            return ReplaceLineFeedWithNewLine(RemoveAniDbLinks(description));
        }

        public IEnumerable<PersonInfo> GetPeople(AniDbSeriesData aniDbSeriesData)
        {
            var characters = aniDbSeriesData.Characters?.Where(c => c.Seiyuu != null)
                .Select(c => new PersonInfo
                {
                    Name = ReverseName(c.Seiyuu.Name),
                    ImageUrl = c.Seiyuu?.PictureUrl,
                    Type = PersonType.Actor,
                    Role = c.Name
                })
                .ToList() ?? new List<PersonInfo>();

            var creators = aniDbSeriesData.Creators?.Where(c =>
            {
                return this._creatorTypeMappings.ContainsKey(c.Type);
            })
            .Select(c => {
                return new PersonInfo
                {
                    Name = ReverseName(c.Name),
                    Type = this._creatorTypeMappings[c.Type]
                };
            })    ?? new List<PersonInfo>();

            return characters.Concat(creators);
        }

        private string ReverseName(string name)
        {
            name = name ?? "";

            return string.Join(" ", name.Split(' ').Reverse());
        }

        private IEnumerable<string> GetGenreTags(IEnumerable<TagData> tags, bool addAnimeGenre)
        {
            tags = addAnimeGenre ? AddAnimeTag(tags) : tags;

            return ExcludeIgnoredTags(tags)
                .Where(t => t.Weight >= 400)
                .OrderByDescending(t => t.Weight)
                .Select(t => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(t.Name));
        }

        private IEnumerable<TagData> AddAnimeTag(IEnumerable<TagData> tags)
        {
            return new[]
            {
                new TagData
                {
                    Name = "Anime",
                    Weight = int.MaxValue
                }
            }.Concat(tags);
        }

        private IEnumerable<TagData> ExcludeIgnoredTags(IEnumerable<TagData> tags)
        {
            var ignoredTagIds = new[] { 6, 22, 23, 60, 128, 129, 185, 216, 242, 255, 268, 269, 289 };

            return tags.Where(t => !ignoredTagIds.Contains(t.Id) && !ignoredTagIds.Contains(t.ParentId));
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