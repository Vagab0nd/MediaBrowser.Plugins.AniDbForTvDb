namespace MediaBrowser.Plugins.AniMetadata.Configuration
{
    public class PropertyMappingDefinition
        {
            public PropertyMappingDefinition()
            {
            }

            public PropertyMappingDefinition(string friendlyName, string sourceName, string targetPropertyName)
            {
                FriendlyName = friendlyName;
                SourceName = sourceName;
                TargetPropertyName = targetPropertyName;
            }

            public string FriendlyName { get; set; }

            public string SourceName { get; set; }

            public string TargetPropertyName { get; set; }
        }
}