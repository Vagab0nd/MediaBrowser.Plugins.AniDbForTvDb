namespace MediaBrowser.Plugins.AniMetadata.Process.Sources
{
    internal class SourceName
    {
        public SourceName(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public static implicit operator string(SourceName sourceName)
        {
            return sourceName.Name;
        }
    }
}