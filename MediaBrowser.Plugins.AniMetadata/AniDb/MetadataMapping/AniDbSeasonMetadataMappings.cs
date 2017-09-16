using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.MetadataMapping;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.MetadataMapping
{
    internal class AniDbSeasonMetadataMappings
    {
        public AniDbSeasonMetadataMappings(IAniDbParser aniDbParser)
        {
            SeasonMappings = new IPropertyMapping<AniDbSeries, MetadataResult<Season>>[]
            {
                MapSeason(t => t.Item.Name, (s, t) => t.Item.Name = s.SelectedTitle),
                MapSeason(t => t.Item.PremiereDate, (s, t) => t.Item.PremiereDate = s.Data.StartDate),
                MapSeason(t => t.Item.EndDate, (s, t) => t.Item.EndDate = s.Data.EndDate),
                MapSeason(t => t.Item.CommunityRating,
                    (s, t) => t.Item.CommunityRating = s.Data.Ratings?.OfType<PermanentRatingData>().Single().Value),
                MapSeason(t => t.Item.Overview,
                    (s, t) => t.Item.Overview = aniDbParser.FormatDescription(s.Data.Description)),
                MapSeason(t => t.Item.Studios, (s, t) => t.Item.Studios = aniDbParser.GetStudios(s.Data).ToArray()),
                MapSeason(t => t.Item.Genres, (s, t) => t.Item.Genres.AddRange(aniDbParser.GetGenres(s.Data))),
                MapSeason(t => t.Item.Tags, (s, t) => t.Item.Tags = aniDbParser.GetTags(s.Data).ToArray())
            };
        }

        public IEnumerable<IPropertyMapping<AniDbSeries, MetadataResult<Season>>> SeasonMappings { get; }

        private static PropertyMapping<AniDbSeries, MetadataResult<Season>, TTargetProperty> MapSeason<TTargetProperty>(
            Expression<Func<MetadataResult<Season>, TTargetProperty>> targetPropertySelector,
            Action<AniDbSeries, MetadataResult<Season>> map)
        {
            return new PropertyMapping<AniDbSeries, MetadataResult<Season>, TTargetProperty>
                (targetPropertySelector, map);
        }
    }
}