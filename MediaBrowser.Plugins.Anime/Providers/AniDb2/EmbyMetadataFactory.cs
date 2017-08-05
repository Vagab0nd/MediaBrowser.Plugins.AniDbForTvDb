using System;
using System.Linq;
using System.Text.RegularExpressions;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.Anime.AniDb.Data;
using MediaBrowser.Plugins.Anime.Configuration;

namespace MediaBrowser.Plugins.Anime.Providers.AniDb2
{
    internal class EmbyMetadataFactory
    {
        private readonly PluginConfiguration _configuration;
        private readonly TitleSelector _titleSelector;

        public EmbyMetadataFactory(TitleSelector titleSelector, PluginConfiguration configuration)
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

        private Series CreateEmbySeries(AniDbSeries aniDbSeries, string selectedTitle)
        {
            var ignoredTagIds = new[] { 6, 22, 23, 60, 128, 129, 185, 216, 242, 255, 268, 269, 289 };

            var embySeries = new Series
            {
                PremiereDate = aniDbSeries.StartDate,
                EndDate = aniDbSeries.EndDate,
                Name = selectedTitle,
                Overview = ReplaceLineFeedWithNewLine(RemoveAniDbLinks(aniDbSeries.Description))
            };

            embySeries.ProviderIds.Add(ProviderNames.AniDb, aniDbSeries.Id.ToString());

            embySeries.Studios.AddRange(aniDbSeries.Creators
                .Where(c => c.Type == "Animation Work")
                .Select(c => c.Name));

            embySeries.CommunityRating = aniDbSeries.Ratings.OfType<PermanentRating>().Single().Value;

            embySeries.Genres.AddRange(aniDbSeries.Tags
                .Where(t => t.Weight >= 400 && !ignoredTagIds.Contains(t.Id) && !ignoredTagIds.Contains(t.ParentId))
                .OrderBy(t => t.Weight)
                .Select(t => t.Name));
            
            return embySeries;
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