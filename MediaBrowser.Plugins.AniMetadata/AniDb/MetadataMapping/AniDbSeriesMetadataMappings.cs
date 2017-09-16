using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.MetadataMapping;
using MediaBrowser.Plugins.AniMetadata.Providers;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.MetadataMapping
{
    internal class AniDbSeriesMetadataMappings
    {
        public AniDbSeriesMetadataMappings(IAniDbParser aniDbParser)
        {
            SeriesMappings = new IPropertyMapping<AniDbSeries, MetadataResult<Series>>[]
            {
                MapSeries(t => t.Item.Name, (s, t) => t.Item.Name = s.SelectedTitle),
                MapSeries(t => t.Item.PremiereDate, (s, t) => t.Item.PremiereDate = s.Data.StartDate),
                MapSeries(t => t.Item.EndDate, (s, t) => t.Item.EndDate = s.Data.EndDate),
                MapSeries(t => t.Item.CommunityRating,
                    (s, t) => t.Item.CommunityRating = s.Data.Ratings?.OfType<PermanentRatingData>().Single().Value),
                MapSeries(t => t.Item.Overview,
                    (s, t) => t.Item.Overview = aniDbParser.FormatDescription(s.Data.Description)),
                MapSeries(t => t.Item.Studios, (s, t) => t.Item.Studios = aniDbParser.GetStudios(s.Data).ToArray()),
                MapSeries(t => t.Item.Genres, (s, t) => t.Item.Genres.AddRange(aniDbParser.GetGenres(s.Data))),
                MapSeries(t => t.Item.Tags, (s, t) => t.Item.Tags = aniDbParser.GetTags(s.Data).ToArray()),
                MapSeries(t => t.People, (s, t) => t.People = aniDbParser.GetPeople(s.Data).ToList()),
                MapSeries(t => t.Item.ProviderIds,
                    (s, t) => t.Item.ProviderIds.Add(ProviderNames.AniDb, s.Data.Id.ToString()))
            };
        }

        public IEnumerable<IPropertyMapping<AniDbSeries, MetadataResult<Series>>> SeriesMappings { get; }

        private static PropertyMapping<AniDbSeries, MetadataResult<Series>, TTargetProperty> MapSeries<TTargetProperty>(
            Expression<Func<MetadataResult<Series>, TTargetProperty>> targetPropertySelector,
            Action<AniDbSeries, MetadataResult<Series>> map)
        {
            return new PropertyMapping<AniDbSeries, MetadataResult<Series>, TTargetProperty>
                (targetPropertySelector, map);
        }
    }
}