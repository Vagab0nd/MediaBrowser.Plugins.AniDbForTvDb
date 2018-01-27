namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal class ProcessFailedResult
    {
        public ProcessFailedResult(string sourceName, string mediaItemName, ItemType mediaItemType, string reason)
        {
            SourceName = sourceName;
            MediaItemName = mediaItemName;
            MediaItemType = mediaItemType;
            Reason = reason;
        }

        public string SourceName { get; }

        public string MediaItemName { get; }

        public ItemType MediaItemType { get; }

        public string Reason { get; }
    }

    internal class ProcessResultContext
    {
        private readonly string _mediaItemName;
        private readonly ItemType _mediaItemType;
        private readonly string _sourceName;

        public ProcessResultContext(string sourceName, string mediaItemName, ItemType mediaItemType)
        {
            _sourceName = sourceName;
            _mediaItemName = mediaItemName;
            _mediaItemType = mediaItemType;
        }

        public ProcessFailedResult Failed(string reason)
        {
            return new ProcessFailedResult(_sourceName, _mediaItemName, _mediaItemType, reason);
        }
    }
}