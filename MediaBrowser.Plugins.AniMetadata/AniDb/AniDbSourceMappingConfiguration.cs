using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using MediaBrowser.Plugins.AniMetadata.PropertyMapping;

namespace MediaBrowser.Plugins.AniMetadata.AniDb
{
    internal class AniDbSourceMappingConfiguration : ISourceMappingConfiguration
    {
        private readonly IAniDbParser _aniDbParser;
        private readonly ITitleSelector _titleSelector;

        public AniDbSourceMappingConfiguration(IAniDbParser aniDbParser, ITitleSelector titleSelector)
        {
            _aniDbParser = aniDbParser;
            _titleSelector = titleSelector;
        }

        public IEnumerable<PropertyMappingDefinition> GetSeriesMappingDefinitions()
        {
            return GetSeriesMappings(0, false, false, TitleType.Localized, "")
                .Select(m => new PropertyMappingDefinition(m.FriendlyName, m.SourceName, m.TargetPropertyName));
        }

        public IEnumerable<IPropertyMapping> GetSeriesMappings(int maxGenres, bool addAnimeGenre,
            bool moveExcessGenresToTags, TitleType preferredTitleType, string metadataLanguage)
        {
            return new IPropertyMapping[]
            {
                MapSeries("Name", t => t.Item.Name,
                    (s, t) => t.Item.Name = SelectTitle(s, preferredTitleType, metadataLanguage)),
                MapSeries("Release date", t => t.Item.PremiereDate, (s, t) => t.Item.PremiereDate = s.StartDate,
                    (s, t) => s.StartDate.HasValue),
                MapSeries("End date", t => t.Item.EndDate, (s, t) => t.Item.EndDate = s.EndDate,
                    (s, t) => s.EndDate.HasValue),
                MapSeries("Community rating", t => t.Item.CommunityRating,
                    (s, t) => t.Item.CommunityRating = s.Ratings?.OfType<PermanentRatingData>().Single().Value,
                    (s, t) => s.Ratings?.OfType<PermanentRatingData>().Count() == 1),
                MapSeries("Overview", t => t.Item.Overview,
                    (s, t) => t.Item.Overview = _aniDbParser.FormatDescription(s.Description),
                    (s, t) => !string.IsNullOrWhiteSpace(s.Description)),
                MapSeries("Studios", t => t.Item.Studios,
                    (s, t) => t.Item.Studios = _aniDbParser.GetStudios(s).ToArray()),
                MapSeries("Genres", t => t.Item.Genres,
                    (s, t) => t.Item.Genres.AddRange(_aniDbParser.GetGenres(s, maxGenres, addAnimeGenre))),
                MapSeries("Tags", t => t.Item.Tags,
                    (s, t) => t.Item.Tags = _aniDbParser.GetTags(s, maxGenres, addAnimeGenre).ToArray()),
                MapSeries("People", t => t.People, (s, t) => t.People = _aniDbParser.GetPeople(s).ToList())
            };
        }

        public IEnumerable<PropertyMappingDefinition> GetSeasonMappingDefinitions()
        {
            return GetSeasonMappings(0, false, TitleType.Localized, "")
                .Select(m => new PropertyMappingDefinition(m.FriendlyName, m.SourceName, m.TargetPropertyName));
        }

        public IEnumerable<IPropertyMapping> GetSeasonMappings(int maxGenres, bool addAnimeGenre,
            TitleType preferredTitleType, string metadataLanguage)
        {
            return new IPropertyMapping[]
            {
                MapSeason("Name", t => t.Item.Name,
                    (s, t) => t.Item.Name = SelectTitle(s, preferredTitleType, metadataLanguage)),
                MapSeason("Release date", t => t.Item.PremiereDate, (s, t) => t.Item.PremiereDate = s.StartDate,
                    (s, t) => s.StartDate.HasValue),
                MapSeason("End date", t => t.Item.EndDate, (s, t) => t.Item.EndDate = s.EndDate,
                    (s, t) => s.EndDate.HasValue),
                MapSeason("Community rating", t => t.Item.CommunityRating,
                    (s, t) => t.Item.CommunityRating = s.Ratings?.OfType<PermanentRatingData>().Single().Value,
                    (s, t) => s.Ratings?.OfType<PermanentRatingData>().Count() == 1),
                MapSeason("Overview", t => t.Item.Overview,
                    (s, t) => t.Item.Overview = _aniDbParser.FormatDescription(s.Description),
                    (s, t) => !string.IsNullOrWhiteSpace(s.Description)),
                MapSeason("Studios", t => t.Item.Studios,
                    (s, t) => t.Item.Studios = _aniDbParser.GetStudios(s).ToArray()),
                MapSeason("Genres", t => t.Item.Genres,
                    (s, t) => t.Item.Genres.AddRange(_aniDbParser.GetGenres(s, maxGenres, addAnimeGenre))),
                MapSeason("Tags", t => t.Item.Tags,
                    (s, t) => t.Item.Tags = _aniDbParser.GetTags(s, maxGenres, addAnimeGenre).ToArray())
            };
        }

        public IEnumerable<PropertyMappingDefinition> GetEpisodeMappingDefinitions()
        {
            return GetEpisodeMappings(0, false, false, TitleType.Localized, "")
                .Select(m => new PropertyMappingDefinition(m.FriendlyName, m.SourceName, m.TargetPropertyName));
        }

        public IEnumerable<IPropertyMapping> GetEpisodeMappings(int maxGenres, bool addAnimeGenre,
            bool moveExcessGenresToTags, TitleType preferredTitleType, string metadataLanguage)
        {
            return new IPropertyMapping[]
            {
                MapEpisode("Name", t => t.Item.Name,
                    (s, t) => t.Item.Name = SelectTitle(s, preferredTitleType, metadataLanguage)),
                MapEpisode("Release date", t => t.Item.PremiereDate, (s, t) => t.Item.PremiereDate = s.AirDate),
                MapEpisode("Runtime", t => t.Item.RunTimeTicks,
                    (s, t) => t.Item.RunTimeTicks = new TimeSpan(0, s.TotalMinutes, 0).Ticks,
                    (s, t) => s.TotalMinutes > 0),
                MapEpisode("Community rating", t => t.Item.CommunityRating,
                    (s, t) => t.Item.CommunityRating = s.Rating?.Rating,
                    (s, t) => s.Rating?.Rating > 0),
                MapEpisode("Overview", t => t.Item.Overview, (s, t) => t.Item.Overview = s.Summary,
                    (s, t) => !string.IsNullOrWhiteSpace(s.Summary)),
                MapEpisodeFromSeriesData("Studios", t => t.Item.Studios,
                    (s, t) => t.Item.Studios = _aniDbParser.GetStudios(s).ToArray()),
                MapEpisodeFromSeriesData("Genres", t => t.Item.Genres,
                    (s, t) => t.Item.Genres.AddRange(_aniDbParser.GetGenres(s, maxGenres, addAnimeGenre))),
                MapEpisodeFromSeriesData("Tags", t => t.Item.Tags,
                    (s, t) => t.Item.Tags = _aniDbParser.GetTags(s, maxGenres, addAnimeGenre).ToArray()),
                MapEpisodeFromSeriesData("People", t => t.People, (s, t) => t.People = _aniDbParser.GetPeople(s).ToList())
            };
        }

        private static PropertyMapping<AniDbSeriesData, MetadataResult<Series>, TTargetProperty> MapSeries<
            TTargetProperty>(string friendlyName,
            Expression<Func<MetadataResult<Series>, TTargetProperty>> targetPropertySelector,
            Action<AniDbSeriesData, MetadataResult<Series>> apply)
        {
            return new PropertyMapping<AniDbSeriesData, MetadataResult<Series>, TTargetProperty>
                (friendlyName, targetPropertySelector, apply, SourceNames.AniDb);
        }

        private static PropertyMapping<AniDbSeriesData, MetadataResult<Series>, TTargetProperty> MapSeries<
            TTargetProperty>(string friendlyName,
            Expression<Func<MetadataResult<Series>, TTargetProperty>> targetPropertySelector,
            Action<AniDbSeriesData, MetadataResult<Series>> apply,
            Func<AniDbSeriesData, MetadataResult<Series>, bool> canApply)
        {
            return new PropertyMapping<AniDbSeriesData, MetadataResult<Series>, TTargetProperty>
                (friendlyName, targetPropertySelector, apply, SourceNames.AniDb, canApply);
        }

        private static PropertyMapping<AniDbSeriesData, MetadataResult<Season>, TTargetProperty> MapSeason<
            TTargetProperty>(string friendlyName,
            Expression<Func<MetadataResult<Season>, TTargetProperty>> targetPropertySelector,
            Action<AniDbSeriesData, MetadataResult<Season>> apply)
        {
            return new PropertyMapping<AniDbSeriesData, MetadataResult<Season>, TTargetProperty>
                (friendlyName, targetPropertySelector, apply, SourceNames.AniDb);
        }

        private static PropertyMapping<AniDbSeriesData, MetadataResult<Season>, TTargetProperty> MapSeason<
            TTargetProperty>(string friendlyName,
            Expression<Func<MetadataResult<Season>, TTargetProperty>> targetPropertySelector,
            Action<AniDbSeriesData, MetadataResult<Season>> apply,
            Func<AniDbSeriesData, MetadataResult<Season>, bool> canApply)
        {
            return new PropertyMapping<AniDbSeriesData, MetadataResult<Season>, TTargetProperty>
                (friendlyName, targetPropertySelector, apply, SourceNames.AniDb, canApply);
        }

        private static PropertyMapping<AniDbEpisodeData, MetadataResult<Episode>, TTargetProperty> MapEpisode<
            TTargetProperty>(string friendlyName,
            Expression<Func<MetadataResult<Episode>, TTargetProperty>> targetPropertySelector,
            Action<AniDbEpisodeData, MetadataResult<Episode>> apply)
        {
            return new PropertyMapping<AniDbEpisodeData, MetadataResult<Episode>, TTargetProperty>
                (friendlyName, targetPropertySelector, apply, SourceNames.AniDb);
        }

        private static PropertyMapping<AniDbEpisodeData, MetadataResult<Episode>, TTargetProperty> MapEpisode<
            TTargetProperty>(string friendlyName,
            Expression<Func<MetadataResult<Episode>, TTargetProperty>> targetPropertySelector,
            Action<AniDbEpisodeData, MetadataResult<Episode>> apply,
            Func<AniDbEpisodeData, MetadataResult<Episode>, bool> canApply)
        {
            return new PropertyMapping<AniDbEpisodeData, MetadataResult<Episode>, TTargetProperty>
                (friendlyName, targetPropertySelector, apply, SourceNames.AniDb, canApply);
        }

        private static PropertyMapping<AniDbSeriesData, MetadataResult<Episode>, TTargetProperty> MapEpisodeFromSeriesData<
            TTargetProperty>(string friendlyName,
            Expression<Func<MetadataResult<Episode>, TTargetProperty>> targetPropertySelector,
            Action<AniDbSeriesData, MetadataResult<Episode>> apply)
        {
            return new PropertyMapping<AniDbSeriesData, MetadataResult<Episode>, TTargetProperty>
                (friendlyName, targetPropertySelector, apply, SourceNames.AniDb);
        }

        private string SelectTitle(AniDbSeriesData aniDbSeriesData, TitleType preferredTitleType,
            string metadataLanguage)
        {
            return _titleSelector.SelectTitle(aniDbSeriesData.Titles, preferredTitleType, metadataLanguage)
                .Match(t => t.Title, () => "");
        }

        private string SelectTitle(AniDbEpisodeData aniDbEpisodeData, TitleType preferredTitleType,
            string metadataLanguage)
        {
            return _titleSelector.SelectTitle(aniDbEpisodeData.Titles, preferredTitleType, metadataLanguage)
                .Match(t => t.Title, () => "");
        }
    }
}