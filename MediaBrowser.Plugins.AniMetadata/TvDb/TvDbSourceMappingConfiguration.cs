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
                .Select(m => new PropertyMappingDefinition(m.FriendlyName, m.SourceName, m.TargetPropertyName));
        }

        public IEnumerable<IPropertyMapping> GetSeriesMappings(int maxGenres, bool addAnimeGenre,
            bool moveExcessGenresToTags, TitleType preferredTitleType, string metadataLanguage)
        {
            return new IPropertyMapping[]
            {
                MapSeries("Name", t => t.Item.Name, (s, t) => t.Item.Name = s.SeriesName),
                MapSeries("Release date", t => t.Item.PremiereDate,
                    (s, t) => t.Item.PremiereDate = s.FirstAired.ToNullable(),
                    (s, t) => s.FirstAired.IsSome),
                MapSeries("Community rating", t => t.Item.CommunityRating,
                    (s, t) => t.Item.CommunityRating = s.SiteRating,
                    (s, t) => s.SiteRating > 0),
                MapSeries("Overview", t => t.Item.Overview, (s, t) => t.Item.Overview = s.Overview,
                    (s, t) => !string.IsNullOrWhiteSpace(s.Overview)),
                MapSeries("Genres", t => t.Item.Genres, (s, t) => t.Item.Genres.AddRange(s.Genres.Take(maxGenres))),
                MapSeries("Air days", t => t.Item.AirDays,
                    (s, t) => s.AirsDayOfWeek.IfSome(d => t.Item.AirDays = d.ToDaysOfWeek().ToArray()),
                    (s, t) => s.AirsDayOfWeek.IsSome),
                MapSeries("Air time", t => t.Item.AirTime, (s, t) => t.Item.AirTime = s.AirsTime),
                MapSeries("Tags", t => t.Item.Tags,
                    (s, t) => t.Item.Tags = moveExcessGenresToTags
                        ? s.Genres.Skip(maxGenres).ToArray()
                        : new string[0])
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
                MapSeason("Name", t => t.Item.Name, (s, t) => t.Item.Name = "Season " + s.SeasonNumber)
            };
        }

        public IEnumerable<PropertyMappingDefinition> GetEpisodeMappingDefinitions()
        {
            return GetEpisodeMappings(TitleType.Localized, "")
                .Select(m => new PropertyMappingDefinition(m.FriendlyName, m.SourceName, m.TargetPropertyName));
        }

        public IEnumerable<IPropertyMapping> GetEpisodeMappings(TitleType preferredTitleType, string metadataLanguage)
        {
            return new IPropertyMapping[]
            {
                MapEpisode("Name", t => t.Item.Name, (s, t) => t.Item.Name = s.EpisodeName),
                MapEpisode("Release date", t => t.Item.PremiereDate,
                    (s, t) => t.Item.PremiereDate = s.FirstAired.ToNullable(),
                    (s, t) => s.FirstAired.IsSome),
                MapEpisode("Community rating", t => t.Item.CommunityRating,
                    (s, t) => t.Item.CommunityRating = s.SiteRating,
                    (s, t) => s.SiteRating > 0),
                MapEpisode("Overview", t => t.Item.Overview, (s, t) => t.Item.Overview = s.Overview,
                    (s, t) => !string.IsNullOrWhiteSpace(s.Overview))
            };
        }

        private static PropertyMapping<TvDbSeriesData, MetadataResult<Series>, TTargetProperty> MapSeries<
            TTargetProperty>(string friendlyName,
            Expression<Func<MetadataResult<Series>, TTargetProperty>> targetPropertySelector,
            Action<TvDbSeriesData, MetadataResult<Series>> apply)
        {
            return new PropertyMapping<TvDbSeriesData, MetadataResult<Series>, TTargetProperty>
                (friendlyName, targetPropertySelector, apply, "TvDB");
        }

        private static PropertyMapping<TvDbSeriesData, MetadataResult<Series>, TTargetProperty> MapSeries<
            TTargetProperty>(string friendlyName,
            Expression<Func<MetadataResult<Series>, TTargetProperty>> targetPropertySelector,
            Action<TvDbSeriesData, MetadataResult<Series>> apply,
            Func<TvDbSeriesData, MetadataResult<Series>, bool> canApply)
        {
            return new PropertyMapping<TvDbSeriesData, MetadataResult<Series>, TTargetProperty>
                (friendlyName, targetPropertySelector, apply, "TvDB", canApply);
        }

        private static PropertyMapping<TvDbSeasonData, MetadataResult<Season>, TTargetProperty> MapSeason<
            TTargetProperty>(string friendlyName,
            Expression<Func<MetadataResult<Season>, TTargetProperty>> targetPropertySelector,
            Action<TvDbSeasonData, MetadataResult<Season>> apply)
        {
            return new PropertyMapping<TvDbSeasonData, MetadataResult<Season>, TTargetProperty>
                (friendlyName, targetPropertySelector, apply, "TvDB");
        }

        private static PropertyMapping<TvDbSeasonData, MetadataResult<Season>, TTargetProperty> MapSeason<
            TTargetProperty>(string friendlyName,
            Expression<Func<MetadataResult<Season>, TTargetProperty>> targetPropertySelector,
            Action<TvDbSeasonData, MetadataResult<Season>> apply,
            Func<TvDbSeasonData, MetadataResult<Season>, bool> canApply)
        {
            return new PropertyMapping<TvDbSeasonData, MetadataResult<Season>, TTargetProperty>
                (friendlyName, targetPropertySelector, apply, "TvDB", canApply);
        }

        private static PropertyMapping<TvDbEpisodeData, MetadataResult<Episode>, TTargetProperty> MapEpisode<
            TTargetProperty>(string friendlyName,
            Expression<Func<MetadataResult<Episode>, TTargetProperty>> targetPropertySelector,
            Action<TvDbEpisodeData, MetadataResult<Episode>> apply)
        {
            return new PropertyMapping<TvDbEpisodeData, MetadataResult<Episode>, TTargetProperty>
                (friendlyName, targetPropertySelector, apply, "TvDB");
        }

        private static PropertyMapping<TvDbEpisodeData, MetadataResult<Episode>, TTargetProperty> MapEpisode<
            TTargetProperty>(string friendlyName,
            Expression<Func<MetadataResult<Episode>, TTargetProperty>> targetPropertySelector,
            Action<TvDbEpisodeData, MetadataResult<Episode>> apply,
            Func<TvDbEpisodeData, MetadataResult<Episode>, bool> canApply)
        {
            return new PropertyMapping<TvDbEpisodeData, MetadataResult<Episode>, TTargetProperty>
                (friendlyName, targetPropertySelector, apply, "TvDB", canApply);
        }
    }
}