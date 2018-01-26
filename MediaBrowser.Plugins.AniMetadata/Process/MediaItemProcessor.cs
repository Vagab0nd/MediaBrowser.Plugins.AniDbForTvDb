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

        public OptionAsync<IEmbyResult> GetResultAsync(ItemLookupInfo embyInfo, ItemType itemType)
        {
            var embyItemData = ToEmbyItemData(embyInfo, itemType);

            var mediaItem = _mediaItemBuilder.IdentifyAsync(embyItemData, itemType);

            var fullyRecognisedMediaItem = mediaItem.Map(_mediaItemBuilder.BuildMediaItemAsync);

            return fullyRecognisedMediaItem.Map(_embyResultFactory.GetResult);
        }

        private EmbyItemData ToEmbyItemData(ItemLookupInfo embyInfo, ItemType itemType)
        {
            var existingIds = embyInfo.ProviderIds.Where(v => int.TryParse(v.Value, out _))
                .ToDictionary(k => k.Key, v => int.Parse(v.Value));

            return new EmbyItemData(itemType,
                new ItemIdentifier(embyInfo.IndexNumber.ToOption(), embyInfo.ParentIndexNumber.ToOption(),
                    embyInfo.Name), existingIds, embyInfo.MetadataLanguage);
        }
    }
}