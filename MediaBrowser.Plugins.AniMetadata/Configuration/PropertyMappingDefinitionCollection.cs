using System.Collections.Generic;
using System.Linq;

namespace MediaBrowser.Plugins.AniMetadata.Configuration
{
    public class PropertyMappingDefinitionCollection
    {
        public PropertyMappingDefinitionCollection()
        {
        }

        public PropertyMappingDefinitionCollection(string targetPropertyName, IEnumerable<PropertyMappingDefinition> mappings)
        {
            TargetPropertyName = targetPropertyName;
            Mappings = mappings.ToArray();
        }

        public string TargetPropertyName { get; set; }

        public PropertyMappingDefinition[] Mappings { get; set; }
    }
}