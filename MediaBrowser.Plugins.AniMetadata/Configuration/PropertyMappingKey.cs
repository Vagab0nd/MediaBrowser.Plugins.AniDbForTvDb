namespace MediaBrowser.Plugins.AniMetadata.Configuration
{
    public class PropertyMappingKey
        {
            public PropertyMappingKey()
            {
            }

            public PropertyMappingKey(string sourceName, string targetPropertyName)
            {
                SourceName = sourceName;
                TargetPropertyName = targetPropertyName;
            }

            public string SourceName { get; set; }

            public string TargetPropertyName { get; set; }
        }
}