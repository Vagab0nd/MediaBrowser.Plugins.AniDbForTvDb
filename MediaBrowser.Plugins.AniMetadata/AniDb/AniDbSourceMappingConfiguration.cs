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

        public AniDbSourceMappingConfiguration(IAniDbParser aniDbParser)
        {
            _aniDbParser = aniDbParser;
        }

        public IEnumerable<IPropertyMapping> GetSeriesMappings(int maxGenres, bool addAnimeGenre,
            bool moveExcessGenresToTags)
        {
            return new IPropertyMapping[]
            {
                MapSeries(t => t.Item.Name, (s, t) => t.Item.Name = s.SelectedTitle),
                MapSeries(t => t.Item.PremiereDate, (s, t) => t.Item.PremiereDate = s.Data.StartDate),
                MapSeries(t => t.Item.EndDate, (s, t) => t.Item.EndDate = s.Data.EndDate),
                MapSeries(t => t.Item.CommunityRating,
                    (s, t) => t.Item.CommunityRating = s.Data.Ratings?.OfType<PermanentRatingData>().Single().Value),
                MapSeries(t => t.Item.Overview,
                    (s, t) => t.Item.Overview = _aniDbParser.FormatDescription(s.Data.Description)),
                MapSeries(t => t.Item.Studios, (s, t) => t.Item.Studios = _aniDbParser.GetStudios(s.Data).ToArray()),
                MapSeries(t => t.Item.Genres,
                    (s, t) => t.Item.Genres.AddRange(_aniDbParser.GetGenres(s.Data, maxGenres, addAnimeGenre))),
                MapSeries(t => t.Item.Tags,
                    (s, t) => t.Item.Tags = _aniDbParser.GetTags(s.Data, maxGenres, addAnimeGenre).ToArray()),
                MapSeries(t => t.People, (s, t) => t.People = _aniDbParser.GetPeople(s.Data).ToList())
            };
        }

        public IEnumerable<IPropertyMapping> GetSeasonMappings(int maxGenres, bool addAnimeGenre)
        {
            return new IPropertyMapping[]
            {
                MapSeason(t => t.Item.Name, (s, t) => t.Item.Name = s.SelectedTitle),
                MapSeason(t => t.Item.PremiereDate, (s, t) => t.Item.PremiereDate = s.Data.StartDate),
                MapSeason(t => t.Item.EndDate, (s, t) => t.Item.EndDate = s.Data.EndDate),
                MapSeason(t => t.Item.CommunityRating,
                    (s, t) => t.Item.CommunityRating = s.Data.Ratings?.OfType<PermanentRatingData>().Single().Value),
                MapSeason(t => t.Item.Overview,
                    (s, t) => t.Item.Overview = _aniDbParser.FormatDescription(s.Data.Description)),
                MapSeason(t => t.Item.Studios, (s, t) => t.Item.Studios = _aniDbParser.GetStudios(s.Data).ToArray()),
                MapSeason(t => t.Item.Genres,
                    (s, t) => t.Item.Genres.AddRange(_aniDbParser.GetGenres(s.Data, maxGenres, addAnimeGenre))),
                MapSeason(t => t.Item.Tags,
                    (s, t) => t.Item.Tags = _aniDbParser.GetTags(s.Data, maxGenres, addAnimeGenre).ToArray())
            };
        }

        public IEnumerable<IPropertyMapping> GetEpisodeMappings()
        {
            return new IPropertyMapping[]
            {
                MapEpisode(t => t.Item.Name, (s, t) => t.Item.Name = s.SelectedTitle),
                MapEpisode(t => t.Item.PremiereDate, (s, t) => t.Item.PremiereDate = s.Data.AirDate),
                MapEpisode(t => t.Item.RunTimeTicks,
                    (s, t) => t.Item.RunTimeTicks = new TimeSpan(0, s.Data.TotalMinutes, 0).Ticks),
                MapEpisode(t => t.Item.CommunityRating, (s, t) => t.Item.CommunityRating = s.Data.Rating?.Rating),
                MapEpisode(t => t.Item.Overview, (s, t) => t.Item.Overview = s.Data.Summary)
            };
        }

        private static PropertyMapping<AniDbSeries, MetadataResult<Series>, TTargetProperty> MapSeries<TTargetProperty>(
            Expression<Func<MetadataResult<Series>, TTargetProperty>> targetPropertySelector,
            Action<AniDbSeries, MetadataResult<Series>> apply)
        {
            return new PropertyMapping<AniDbSeries, MetadataResult<Series>, TTargetProperty>
                (targetPropertySelector, apply, ProviderNames.AniDb);
        }

        private static PropertyMapping<AniDbSeries, MetadataResult<Season>, TTargetProperty> MapSeason<TTargetProperty>(
            Expression<Func<MetadataResult<Season>, TTargetProperty>> targetPropertySelector,
            Action<AniDbSeries, MetadataResult<Season>> apply)
        {
            return new PropertyMapping<AniDbSeries, MetadataResult<Season>, TTargetProperty>
                (targetPropertySelector, apply, ProviderNames.AniDb);
        }

        private static PropertyMapping<AniDbEpisode, MetadataResult<Episode>, TTargetProperty> MapEpisode<
            TTargetProperty>(Expression<Func<MetadataResult<Episode>, TTargetProperty>> targetPropertySelector,
            Action<AniDbEpisode, MetadataResult<Episode>> apply)
        {
            return new PropertyMapping<AniDbEpisode, MetadataResult<Episode>, TTargetProperty>
                (targetPropertySelector, apply, ProviderNames.AniDb);
        }
    }
}