using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Controller.Providers;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal class MediaItemProcessor : IMediaItemProcessor
    {
        private readonly IResultFactory _resultFactory;
        private readonly IMediaItemBuilder _mediaItemBuilder;

        public MediaItemProcessor(IMediaItemBuilder mediaItemBuilder, IResultFactory resultFactory)
        {
            _mediaItemBuilder = mediaItemBuilder;
            _resultFactory = resultFactory;
        }

        public Task<Either<ProcessFailedResult, IMetadataFoundResult>> GetResultAsync(ItemLookupInfo embyInfo, ItemType itemType)
        {
            var embyItemData = ToEmbyItemData(embyInfo, itemType);
            
            var mediaItem = _mediaItemBuilder.IdentifyAsync(embyItemData, itemType);

            var fullyRecognisedMediaItem = mediaItem.BindAsync(_mediaItemBuilder.BuildMediaItemAsync);

            return fullyRecognisedMediaItem.BindAsync(mi => _resultFactory.GetResult(mi));
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