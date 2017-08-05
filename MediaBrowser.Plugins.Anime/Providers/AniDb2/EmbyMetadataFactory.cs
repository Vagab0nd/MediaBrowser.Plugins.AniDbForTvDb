using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.Anime.AniDb.Data;
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