using System.Collections.Generic;
using MediaBrowser.Controller.Entities;

namespace MediaBrowser.Plugins.AniMetadata.MetadataMapping
{
    internal static class MetadataMapping<TMetadata> where TMetadata : BaseItem
    {
        public static MetadataMapping<TAniDbSource, TTvDbSource, TMetadata> Create<TAniDbSource, TTvDbSource>(
            IEnumerable<IPropertyMapping<TAniDbSource, TMetadata>> aniDbPropertyMappings,
            IEnumerable<IPropertyMapping<TTvDbSource, TMetadata>> tvDbPropertyMappings)
        {
            return new MetadataMapping<TAniDbSource, TTvDbSource, TMetadata>(aniDbPropertyMappings,
                tvDbPropertyMappings);
        }
    }

    /// <summary>
    ///     Maps data from AniDb and TvDb sources to a target metadata object according to the provided property mappings
    /// </summary>
    internal class MetadataMapping<TAniDbSource, TTvDbSource, TMetadata> where TMetadata : BaseItem
    {
        private readonly IEnumerable<IPropertyMapping<TAniDbSource, TMetadata>> _aniDbPropertyMappings;
        private readonly IEnumerable<IPropertyMapping<TTvDbSource, TMetadata>> _tvDbPropertyMappings;

        public MetadataMapping(IEnumerable<IPropertyMapping<TAniDbSource, TMetadata>> aniDbPropertyMappings,
            IEnumerable<IPropertyMapping<TTvDbSource, TMetadata>> tvDbPropertyMappings)
        {
            _aniDbPropertyMappings = aniDbPropertyMappings;
            _tvDbPropertyMappings = tvDbPropertyMappings;
        }

        public void Map(TAniDbSource aniDbSource, TTvDbSource tvDbSource, TMetadata targetMetadata)
        {
            _aniDbPropertyMappings.Iter(m => m.Map(aniDbSource, targetMetadata));
            _tvDbPropertyMappings.Iter(m => m.Map(tvDbSource, targetMetadata));
        }
    }
}