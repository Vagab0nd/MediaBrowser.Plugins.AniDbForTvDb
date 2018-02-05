namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal class ProcessResultContext
    {
        private readonly string _mediaItemName;
        private readonly IMediaItemType _mediaItemType;
        private readonly string _sourceName;

        public ProcessResultContext(string sourceName, string mediaItemName, IMediaItemType mediaItemType)
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