using System.Collections.Generic;
using System.Linq;

namespace MediaBrowser.Plugins.AniMetadata.Configuration
{
    public class PropertyMappingDefinitionCollection
    {
        public PropertyMappingDefinitionCollection()
        {
        }

        public PropertyMappingDefinitionCollection(string friendlyName, IEnumerable<PropertyMappingDefinition> mappings)
        {
            FriendlyName = friendlyName;
            Mappings = mappings.ToArray();
        }

        public string FriendlyName { get; }
        
        public PropertyMappingDefinition[] Mappings { get; set; }
    }
}