using System.Linq;
using LanguageExt;
using MediaBrowser.Controller.Providers;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal class MediaItemProcessor
    {
        private readonly IEmbyResultFactory _embyResultFactory;
        private readonly IMediaItemBuilder _mediaItemBuilder;

        public MediaItemProcessor(IMediaItemBuilder mediaItemBuilder, IEmbyResultFactory embyResultFactory)
        {
            _mediaItemBuilder = mediaItemBuilder;
            _embyResultFactory = embyResultFactory;
        }

        public Option<IEmbyResult> GetResult(ItemLookupInfo embyInfo, ItemType itemType)
        {
            var embyItemData = ToEmbyItemData(embyInfo);

            var mediaItem = _mediaItemBuilder.Identify(embyItemData, itemType);

            var fullyRecognisedMediaItem = mediaItem.Map(_mediaItemBuilder.BuildMediaItem);

            return fullyRecognisedMediaItem.Map(_embyResultFactory.GetResult);
        }

        private EmbyItemData ToEmbyItemData(ItemLookupInfo embyInfo)
        {
            var existingIds = embyInfo.ProviderIds.Where(v => int.TryParse(v.Value, out _))
                .ToDictionary(k => k.Key, v => int.Parse(v.Value));

            return new EmbyItemData(new ItemIdentifier(embyInfo.IndexNumber, embyInfo.ParentIndexNumber, embyInfo.Name),
                existingIds);
        }
    }
}