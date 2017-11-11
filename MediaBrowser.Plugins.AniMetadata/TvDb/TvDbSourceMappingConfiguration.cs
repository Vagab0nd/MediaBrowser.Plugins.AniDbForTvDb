using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.PropertyMapping;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.TvDb
{
    internal class TvDbSourceMappingConfiguration : ISourceMappingConfiguration
    {
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
                MapSeries(t => t.Item.PremiereDate, (s, t) => t.Item.PremiereDate = s.FirstAired),
                MapSeries(t => t.Item.CommunityRating, (s, t) => t.Item.CommunityRating = s.SiteRating),
                MapSeries(t => t.Item.Overview, (s, t) => t.Item.Overview = s.Overview),
                MapSeries(t => t.Item.Genres, (s, t) => t.Item.Genres.AddRange(s.Genres.Take(maxGenres))),
                MapSeries(t => t.Item.AirDays, (s, t) => t.Item.AirDays = new[] { s.AirsDayOfWeek }),
                MapSeries(t => t.Item.AirTime, (s, t) => t.Item.AirTime = s.AirsTime),
                MapSeries(t => t.Item.Tags,
                    (s, t) => t.Item.Tags = moveExcessGenresToTags
                        ? s.Genres.Skip(maxGenres).ToArray()
                        : new string[0])
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
                MapSeason(t => t.Item.Name, (s, t) => t.Item.Name = "Season " + s.SeasonNumber)
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
                MapEpisode(t => t.Item.PremiereDate, (s, t) => t.Item.PremiereDate = s.FirstAired),
                MapEpisode(t => t.Item.CommunityRating, (s, t) => t.Item.CommunityRating = s.SiteRating),
                MapEpisode(t => t.Item.Overview, (s, t) => t.Item.Overview = s.Overview)
            };
        }

        private static PropertyMapping<TvDbSeriesData, MetadataResult<Series>, TTargetProperty> MapSeries<
            TTargetProperty>(
            Expression<Func<MetadataResult<Series>, TTargetProperty>> targetPropertySelector,
            Action<TvDbSeriesData, MetadataResult<Series>> apply)
        {
            return new PropertyMapping<TvDbSeriesData, MetadataResult<Series>, TTargetProperty>
                (targetPropertySelector, apply, "TvDB");
        }

        private static PropertyMapping<TvDbSeasonData, MetadataResult<Season>, TTargetProperty> MapSeason<
            TTargetProperty>(
            Expression<Func<MetadataResult<Season>, TTargetProperty>> targetPropertySelector,
            Action<TvDbSeasonData, MetadataResult<Season>> apply)
        {
            return new PropertyMapping<TvDbSeasonData, MetadataResult<Season>, TTargetProperty>
                (targetPropertySelector, apply, "TvDB");
        }

        private static PropertyMapping<TvDbEpisodeData, MetadataResult<Episode>, TTargetProperty> MapEpisode<
            TTargetProperty>(
            Expression<Func<MetadataResult<Episode>, TTargetProperty>> targetPropertySelector,
            Action<TvDbEpisodeData, MetadataResult<Episode>> apply)
        {
            return new PropertyMapping<TvDbEpisodeData, MetadataResult<Episode>, TTargetProperty>
                (targetPropertySelector, apply, "TvDB");
        }
    }
}