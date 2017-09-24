using System.Collections.Generic;
using System.Linq;

namespace MediaBrowser.Plugins.AniMetadata.MetadataMapping
{
    /// <summary>
    ///     Maps data from sources to a target metadata object according to the provided property mappings
    /// </summary>
    internal class MetadataMapping : IMetadataMapping
    {
        private readonly IEnumerable<IPropertyMapping> _propertyMappings;

        public MetadataMapping(IEnumerable<IPropertyMapping> propertyMappings)
        {
            _propertyMappings = propertyMappings;
        }

        public TMetadata Apply<TMetadata>(object source, TMetadata target)
        {
            _propertyMappings.Filter(m => m.CanApply(source, target)).Iter(m => m.Apply(source, target));

            return target;
        }

        public TMetadata Apply<TMetadata>(IEnumerable<object> sources, TMetadata target)
        {
            return sources.Aggregate(target, (t, s) => Apply(s, t));
        }
    }
}