using System;
using System.Linq;
using LanguageExt;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.PropertyMapping;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal class MediaItemType<TEmbyItem> : IMediaItemType where TEmbyItem : BaseItem, new()
    {
        private readonly Func<IPluginConfiguration, string, IPropertyMappingCollection> _propertyMappingsFactory;

        internal MediaItemType(MediaItemTypeValue itemType,
            Func<IPluginConfiguration, string, IPropertyMappingCollection> propertyMappingsFactory)
        {
            _propertyMappingsFactory = propertyMappingsFactory;
            Type = itemType;
        }

        public MediaItemTypeValue Type { get; }

        public Either<ProcessFailedResult, IMetadataFoundResult> CreateMetadataFoundResult(
            IPluginConfiguration pluginConfiguration, IMediaItem mediaItem)
        {
            var metadataResult = new MetadataResult<TEmbyItem>
            {
                Item = new TEmbyItem()
            };

            var propertyMappings = _propertyMappingsFactory(pluginConfiguration, mediaItem.EmbyData.Language);
            var sourceData = mediaItem.GetAllSourceData().Select(sd => sd.GetData<object>()).Somes();

            metadataResult = propertyMappings.Apply(sourceData, metadataResult, s => { });

            var mappedMetadataResult = Option<MetadataResult<TEmbyItem>>.Some(metadataResult);

            mappedMetadataResult.IfSome(r =>
                metadataResult.HasMetadata = !string.IsNullOrEmpty(metadataResult.Item.Name));

            return mappedMetadataResult.ToEither(new ProcessFailedResult("PropertyMapping",
                    mediaItem.EmbyData.Identifier.Name, mediaItem.ItemType, "Property mapping returned no data"))
                .Map(r => (IMetadataFoundResult)new MetadataFoundResult<TEmbyItem>(mediaItem, metadataResult));
        }
    }
}