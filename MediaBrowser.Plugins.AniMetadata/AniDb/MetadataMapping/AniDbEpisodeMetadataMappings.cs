using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.MetadataMapping;
using MediaBrowser.Plugins.AniMetadata.Providers;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.MetadataMapping
{
    internal class AniDbEpisodeMetadataMappings
    {
        public AniDbEpisodeMetadataMappings()
        {
            EpisodeMappings = new IPropertyMapping[]
            {
                MapEpisode(t => t.Item.Name, (s, t) => t.Item.Name = s.SelectedTitle),
                MapEpisode(t => t.Item.PremiereDate, (s, t) => t.Item.PremiereDate = s.Data.AirDate),
                MapEpisode(t => t.Item.RunTimeTicks,
                    (s, t) => t.Item.RunTimeTicks = new TimeSpan(0, s.Data.TotalMinutes, 0).Ticks),
                MapEpisode(t => t.Item.CommunityRating, (s, t) => t.Item.CommunityRating = s.Data.Rating?.Rating),
                MapEpisode(t => t.Item.Overview, (s, t) => t.Item.Overview = s.Data.Summary)
            };
        }

        public IEnumerable<IPropertyMapping> EpisodeMappings { get; }

        private static PropertyMapping<AniDbEpisode, MetadataResult<Episode>, TTargetProperty> MapEpisode<
            TTargetProperty>(
            Expression<Func<MetadataResult<Episode>, TTargetProperty>> targetPropertySelector,
            Action<AniDbEpisode, MetadataResult<Episode>> apply)
        {
            return new PropertyMapping<AniDbEpisode, MetadataResult<Episode>, TTargetProperty>
                (targetPropertySelector, apply, ProviderNames.AniDb);
        }
    }
}