using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MediaBrowser.Plugins.AniMetadata.PropertyMapping
{
    internal class PropertyMappingCollection : IPropertyMappingCollection
    {
        private readonly IEnumerable<IPropertyMapping> _propertyMappings;

        public PropertyMappingCollection(IEnumerable<IPropertyMapping> propertyMappings)
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

        public IEnumerator<IPropertyMapping> GetEnumerator()
        {
            return _propertyMappings.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}