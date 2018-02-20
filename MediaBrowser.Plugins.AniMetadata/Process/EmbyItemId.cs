namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal class EmbyItemId
    {
        public EmbyItemId(IMediaItemType itemType, string sourceName, int id)
        {
            ItemType = itemType;
            SourceName = sourceName;
            Id = id;
        }

        public IMediaItemType ItemType { get; }

        public string SourceName { get; }

        public int Id { get; }
    }
}