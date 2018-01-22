using System;
using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal class MediaItemBuilder : IMediaItemBuilder
    {
        private readonly INewPluginConfiguration _pluginConfiguration;

        public MediaItemBuilder(INewPluginConfiguration pluginConfiguration)
        {
            _pluginConfiguration = pluginConfiguration;
        }

        public Option<IMediaItem> Identify(EmbyItemData embyItemData, ItemType itemType)
        {
            return Identify(embyItemData).Map<IMediaItem>(sd => new MediaItem(itemType, sd));
        }

        public IMediaItem BuildMediaItem(IMediaItem mediaItem)
        {
            throw new NotImplementedException();
        }

        private Option<ISourceData> Identify(EmbyItemData embyItemData)
        {
            return embyItemData.IsFileData
                ? _pluginConfiguration.FileStructureSource.Lookup(embyItemData)
                : _pluginConfiguration.LibraryStructureSource.Lookup(embyItemData);
        }
    }
}