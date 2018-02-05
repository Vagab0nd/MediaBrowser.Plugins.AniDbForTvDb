namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal class ProcessFailedResult
    {
        public ProcessFailedResult(string sourceName, string mediaItemName, IMediaItemType mediaItemType, string reason)
        {
            SourceName = sourceName;
            MediaItemName = mediaItemName;
            MediaItemType = mediaItemType;
            Reason = reason;
        }

        public string SourceName { get; }

        public string MediaItemName { get; }

        public IMediaItemType MediaItemType { get; }

        public string Reason { get; }
    }
}