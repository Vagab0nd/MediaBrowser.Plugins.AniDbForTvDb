using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Configuration;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    internal class AniDbParser : IAniDbParser
    {
        private readonly PluginConfiguration _configuration;

        private readonly Dictionary<string, string> _creatorTypeMappings = new Dictionary<string, string>
        {
            { "Direction", PersonType.Director },
            { "Music", PersonType.Composer }
        };

        public AniDbParser(PluginConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IEnumerable<string> GetStudios(AniDbSeriesData aniDbSeriesData)
        {
            return aniDbSeriesData.Creators.Where(c => c.Type == "Animation Work").Select(c => c.Name);
        }

        public IEnumerable<string> GetGenres(AniDbSeriesData aniDbSeriesData)
        {
            return GetGenreTags(aniDbSeriesData.Tags ?? Enumerable.Empty<TagData>()).Take(_configuration.MaxGenres);
        }

        public IEnumerable<string> GetTags(AniDbSeriesData aniDbSeriesData)
        {
            return _configuration.MoveExcessGenresToTags
                ? GetGenreTags(aniDbSeriesData.Tags ?? Enumerable.Empty<TagData>()).Skip(_configuration.MaxGenres)
                : Enumerable.Empty<string>();
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

            var creators = aniDbSeriesData.Creators?.Select(c =>
            {
                var type = _creatorTypeMappings.ContainsKey(c.Type ?? "") ? _creatorTypeMappings[c.Type] : c.Type;

                return new PersonInfo
                {
                    Name = ReverseName(c.Name),
                    Type = type
                };
            }) ?? new List<PersonInfo>();

            return characters.Concat(creators);
        }

        private string ReverseName(string name)
        {
            name = name ?? "";

            return string.Join(" ", name.Split(' ').Reverse());
        }

        private IEnumerable<string> GetGenreTags(IEnumerable<TagData> tags)
        {
            return ExcludeIgnoredTags(AddAnimeTag(tags))
                .Where(t => t.Weight >= 400)
                .OrderByDescending(t => t.Weight)
                .Select(t => t.Name);
        }

        private IEnumerable<TagData> AddAnimeTag(IEnumerable<TagData> tags)
        {
            return _configuration.AddAnimeGenre
                ? new[]
                {
                    new TagData
                    {
                        Name = "Anime",
                        Weight = int.MaxValue
                    }
                }.Concat(tags)
                : tags;
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