using System.Collections.Generic;

namespace MediaBrowser.Plugins.AniMetadata.MetadataMapping
{
    internal static class MetadataMapping<TMetadata>
    {
        public static MetadataMapping<TAniDbSource, TTvDbSource, TMetadata> Create<TAniDbSource, TTvDbSource>(
            IEnumerable<IPropertyMapping> aniDbPropertyMappings,
            IEnumerable<IPropertyMapping> tvDbPropertyMappings)
        {
            return new MetadataMapping<TAniDbSource, TTvDbSource, TMetadata>(aniDbPropertyMappings,
                tvDbPropertyMappings);
        }
    }

    /// <summary>
    ///     Maps data from AniDb and TvDb sources to a target metadata object according to the provided property mappings
    /// </summary>
    internal class MetadataMapping<TAniDbSource, TTvDbSource, TMetadata>
    {
        private readonly IEnumerable<IPropertyMapping> _aniDbPropertyMappings;
        private readonly IEnumerable<IPropertyMapping> _tvDbPropertyMappings;

        public MetadataMapping(IEnumerable<IPropertyMapping> aniDbPropertyMappings,
            IEnumerable<IPropertyMapping> tvDbPropertyMappings)
        {
            _aniDbPropertyMappings = aniDbPropertyMappings;
            _tvDbPropertyMappings = tvDbPropertyMappings;
        }

        public void Apply(TAniDbSource aniDbSource, TTvDbSource tvDbSource, TMetadata targetMetadata)
        {
            _aniDbPropertyMappings.Iter(m => m.Apply(aniDbSource, targetMetadata));
            _tvDbPropertyMappings.Iter(m => m.Apply(tvDbSource, targetMetadata));
        }
    }
}