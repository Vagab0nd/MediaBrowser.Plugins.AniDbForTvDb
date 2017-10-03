namespace MediaBrowser.Plugins.AniMetadata.Configuration
{
    public class PropertyMappingDefinition
        {
            public PropertyMappingDefinition()
            {
            }

            public PropertyMappingDefinition(string sourceName, string targetPropertyName)
            {
                SourceName = sourceName;
                TargetPropertyName = targetPropertyName;
            }

            public string SourceName { get; set; }

            public string TargetPropertyName { get; set; }
        }
}