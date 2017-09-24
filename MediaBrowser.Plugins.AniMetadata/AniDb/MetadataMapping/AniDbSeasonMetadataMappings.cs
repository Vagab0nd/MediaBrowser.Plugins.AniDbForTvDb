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
    internal class AniDbSeasonMetadataMappings
    {
        private readonly IAniDbParser _aniDbParser;

        public AniDbSeasonMetadataMappings(IAniDbParser aniDbParser)
        {
            _aniDbParser = aniDbParser;
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

        private static PropertyMapping<AniDbSeries, MetadataResult<Season>, TTargetProperty> MapSeason<TTargetProperty>(
            Expression<Func<MetadataResult<Season>, TTargetProperty>> targetPropertySelector,
            Action<AniDbSeries, MetadataResult<Season>> apply)
        {
            return new PropertyMapping<AniDbSeries, MetadataResult<Season>, TTargetProperty>
                (targetPropertySelector, apply, ProviderNames.AniDb);
        }
    }
}