using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.MetadataMapping;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.TvDb.MetadataMapping
{
    internal class TvDbSeriesMetadataMappings
    {
        public IEnumerable<IPropertyMapping> GetSeriesMappings(int maxGenres, bool moveExcessGenresToTags)
        {
            return new IPropertyMapping[]
            {
                MapSeries(t => t.Item.Name, (s, t) => t.Item.Name = s.SeriesName),
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

        private static PropertyMapping<TvDbSeriesData, MetadataResult<Series>, TTargetProperty> MapSeries<
            TTargetProperty>(
            Expression<Func<MetadataResult<Series>, TTargetProperty>> targetPropertySelector,
            Action<TvDbSeriesData, MetadataResult<Series>> apply)
        {
            return new PropertyMapping<TvDbSeriesData, MetadataResult<Series>, TTargetProperty>
                (targetPropertySelector, apply, "TvDB");
        }
    }
}