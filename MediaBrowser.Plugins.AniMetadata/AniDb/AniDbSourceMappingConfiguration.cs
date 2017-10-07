using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.PropertyMapping;
using MediaBrowser.Plugins.AniMetadata.Providers;

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
                .Select(m => new PropertyMappingDefinition(m.SourceName, m.TargetPropertyName));
        }

        public IEnumerable<IPropertyMapping> GetSeriesMappings(int maxGenres, bool addAnimeGenre,
            bool moveExcessGenresToTags, TitleType preferredTitleType, string metadataLanguage)
        {
            return new IPropertyMapping[]
            {
                MapSeries(t => t.Item.Name,
                    (s, t) => t.Item.Name = SelectTitle(s, preferredTitleType, metadataLanguage)),
                MapSeries(t => t.Item.PremiereDate, (s, t) => t.Item.PremiereDate = s.StartDate),
                MapSeries(t => t.Item.EndDate, (s, t) => t.Item.EndDate = s.EndDate),
                MapSeries(t => t.Item.CommunityRating,
                    (s, t) => t.Item.CommunityRating = s.Ratings?.OfType<PermanentRatingData>().Single().Value),
                MapSeries(t => t.Item.Overview,
                    (s, t) => t.Item.Overview = _aniDbParser.FormatDescription(s.Description)),
                MapSeries(t => t.Item.Studios, (s, t) => t.Item.Studios = _aniDbParser.GetStudios(s).ToArray()),
                MapSeries(t => t.Item.Genres,
                    (s, t) => t.Item.Genres.AddRange(_aniDbParser.GetGenres(s, maxGenres, addAnimeGenre))),
                MapSeries(t => t.Item.Tags,
                    (s, t) => t.Item.Tags = _aniDbParser.GetTags(s, maxGenres, addAnimeGenre).ToArray()),
                MapSeries(t => t.People, (s, t) => t.People = _aniDbParser.GetPeople(s).ToList())
            };
        }

        public IEnumerable<PropertyMappingDefinition> GetSeasonMappingDefinitions()
        {
            return GetSeasonMappings(0, false, TitleType.Localized, "")
                .Select(m => new PropertyMappingDefinition(m.SourceName, m.TargetPropertyName));
        }

        public IEnumerable<IPropertyMapping> GetSeasonMappings(int maxGenres, bool addAnimeGenre,
            TitleType preferredTitleType, string metadataLanguage)
        {
            return new IPropertyMapping[]
            {
                MapSeason(t => t.Item.Name,
                    (s, t) => t.Item.Name = SelectTitle(s, preferredTitleType, metadataLanguage)),
                MapSeason(t => t.Item.PremiereDate, (s, t) => t.Item.PremiereDate = s.StartDate),
                MapSeason(t => t.Item.EndDate, (s, t) => t.Item.EndDate = s.EndDate),
                MapSeason(t => t.Item.CommunityRating,
                    (s, t) => t.Item.CommunityRating = s.Ratings?.OfType<PermanentRatingData>().Single().Value),
                MapSeason(t => t.Item.Overview,
                    (s, t) => t.Item.Overview = _aniDbParser.FormatDescription(s.Description)),
                MapSeason(t => t.Item.Studios, (s, t) => t.Item.Studios = _aniDbParser.GetStudios(s).ToArray()),
                MapSeason(t => t.Item.Genres,
                    (s, t) => t.Item.Genres.AddRange(_aniDbParser.GetGenres(s, maxGenres, addAnimeGenre))),
                MapSeason(t => t.Item.Tags,
                    (s, t) => t.Item.Tags = _aniDbParser.GetTags(s, maxGenres, addAnimeGenre).ToArray())
            };
        }

        public IEnumerable<PropertyMappingDefinition> GetEpisodeMappingDefinitions()
        {
            return GetEpisodeMappings(TitleType.Localized, "")
                .Select(m => new PropertyMappingDefinition(m.SourceName, m.TargetPropertyName));
        }

        public IEnumerable<IPropertyMapping> GetEpisodeMappings(TitleType preferredTitleType, string metadataLanguage)
        {
            return new IPropertyMapping[]
            {
                MapEpisode(t => t.Item.Name,
                    (s, t) => t.Item.Name = SelectTitle(s, preferredTitleType, metadataLanguage)),
                MapEpisode(t => t.Item.PremiereDate, (s, t) => t.Item.PremiereDate = s.AirDate),
                MapEpisode(t => t.Item.RunTimeTicks,
                    (s, t) => t.Item.RunTimeTicks = new TimeSpan(0, s.TotalMinutes, 0).Ticks),
                MapEpisode(t => t.Item.CommunityRating, (s, t) => t.Item.CommunityRating = s.Rating?.Rating),
                MapEpisode(t => t.Item.Overview, (s, t) => t.Item.Overview = s.Summary)
            };
        }

        private static PropertyMapping<AniDbSeriesData, MetadataResult<Series>, TTargetProperty> MapSeries<
            TTargetProperty>(
            Expression<Func<MetadataResult<Series>, TTargetProperty>> targetPropertySelector,
            Action<AniDbSeriesData, MetadataResult<Series>> apply)
        {
            return new PropertyMapping<AniDbSeriesData, MetadataResult<Series>, TTargetProperty>
                (targetPropertySelector, apply, ProviderNames.AniDb);
        }

        private static PropertyMapping<AniDbSeriesData, MetadataResult<Season>, TTargetProperty> MapSeason<
            TTargetProperty>(
            Expression<Func<MetadataResult<Season>, TTargetProperty>> targetPropertySelector,
            Action<AniDbSeriesData, MetadataResult<Season>> apply)
        {
            return new PropertyMapping<AniDbSeriesData, MetadataResult<Season>, TTargetProperty>
                (targetPropertySelector, apply, ProviderNames.AniDb);
        }

        private static PropertyMapping<AniDbEpisodeData, MetadataResult<Episode>, TTargetProperty> MapEpisode<
            TTargetProperty>(Expression<Func<MetadataResult<Episode>, TTargetProperty>> targetPropertySelector,
            Action<AniDbEpisodeData, MetadataResult<Episode>> apply)
        {
            return new PropertyMapping<AniDbEpisodeData, MetadataResult<Episode>, TTargetProperty>
                (targetPropertySelector, apply, ProviderNames.AniDb);
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