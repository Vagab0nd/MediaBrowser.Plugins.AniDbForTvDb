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
        private readonly IAniDbParser _aniDbParser;

        public AniDbSeriesMetadataMappings(IAniDbParser aniDbParser)
        {
            _aniDbParser = aniDbParser;
        }

        public IEnumerable<IPropertyMapping> GetSeriesMappings(int maxGenres, bool addAnimeGenre)
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

        private static PropertyMapping<AniDbSeries, MetadataResult<Series>, TTargetProperty> MapSeries<TTargetProperty>(
            Expression<Func<MetadataResult<Series>, TTargetProperty>> targetPropertySelector,
            Action<AniDbSeries, MetadataResult<Series>> apply)
        {
            return new PropertyMapping<AniDbSeries, MetadataResult<Series>, TTargetProperty>
                (targetPropertySelector, apply, ProviderNames.AniDb);
        }
    }
}