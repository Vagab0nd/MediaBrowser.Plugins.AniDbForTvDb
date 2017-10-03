using System.Collections.Generic;
using System.Linq;

namespace MediaBrowser.Plugins.AniMetadata.Configuration
{
    public class PropertyMappingKeyCollection
    {
        public PropertyMappingKeyCollection()
        {
        }

        public PropertyMappingKeyCollection(string targetPropertyName, IEnumerable<PropertyMappingDefinition> mappings)
        {
            TargetPropertyName = targetPropertyName;
            Mappings = mappings.ToArray();
        }

        public string TargetPropertyName { get; set; }

        public PropertyMappingDefinition[] Mappings { get; set; }
    }
}