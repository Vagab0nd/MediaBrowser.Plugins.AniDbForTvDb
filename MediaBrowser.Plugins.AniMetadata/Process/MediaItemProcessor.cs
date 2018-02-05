using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.Configuration;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal class MediaItemProcessor : IMediaItemProcessor
    {
        private readonly IMediaItemBuilder _mediaItemBuilder;
        private readonly IPluginConfiguration _pluginConfiguration;

        public MediaItemProcessor(IPluginConfiguration pluginConfiguration, IMediaItemBuilder mediaItemBuilder)
        {
            _pluginConfiguration = pluginConfiguration;
            _mediaItemBuilder = mediaItemBuilder;
        }

        public Task<Either<ProcessFailedResult, IMetadataFoundResult>> GetResultAsync(ItemLookupInfo embyInfo,
            IMediaItemType itemType)
        {
            var embyItemData = ToEmbyItemData(embyInfo, itemType);

            var mediaItem = _mediaItemBuilder.IdentifyAsync(embyItemData, itemType);

            var fullyRecognisedMediaItem = mediaItem.BindAsync(_mediaItemBuilder.BuildMediaItemAsync);

            return fullyRecognisedMediaItem.BindAsync(
                mi => itemType.CreateMetadataFoundResult(_pluginConfiguration, mi));
        }

        private EmbyItemData ToEmbyItemData(ItemLookupInfo embyInfo, IMediaItemType itemType)
        {
            var existingIds = embyInfo.ProviderIds.Where(v => int.TryParse(v.Value, out _))
                .ToDictionary(k => k.Key, v => int.Parse(v.Value));

            return new EmbyItemData(itemType,
                new ItemIdentifier(embyInfo.IndexNumber.ToOption(), embyInfo.ParentIndexNumber.ToOption(),
                    embyInfo.Name), existingIds, embyInfo.MetadataLanguage);
        }
    }
}