using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.PropertyMapping;
using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal class MediaItemType<TEmbyItem> : IMediaItemType<TEmbyItem> where TEmbyItem : BaseItem, new()
    {
        private readonly Func<IPluginConfiguration, string, IPropertyMappingCollection> _propertyMappingsFactory;

        internal MediaItemType(MediaItemTypeValue itemType,
            Func<IPluginConfiguration, string, IPropertyMappingCollection> propertyMappingsFactory)
        {
            _propertyMappingsFactory = propertyMappingsFactory;
            Type = itemType;
        }

        public MediaItemTypeValue Type { get; }

        public Either<ProcessFailedResult, IMetadataFoundResult<TEmbyItem>> CreateMetadataFoundResult(
            IPluginConfiguration pluginConfiguration, IMediaItem mediaItem)
        {
            var metadataResult = new MetadataResult<TEmbyItem>
            {
                Item = new TEmbyItem(),
                HasMetadata = true
            };

            var propertyMappings = _propertyMappingsFactory(pluginConfiguration, mediaItem.EmbyData.Language);
            var sourceData = mediaItem.GetAllSourceData().ToList();

            var mediaItemMetadata = sourceData.Select(sd => sd.GetData<object>()).Somes();

            metadataResult = propertyMappings.Apply(mediaItemMetadata, metadataResult, s => { });

            metadataResult = UpdateProviderIds(metadataResult, sourceData);

            var mappedMetadataResult = Option<MetadataResult<TEmbyItem>>.Some(metadataResult);

            return mappedMetadataResult.ToEither(new ProcessFailedResult("PropertyMapping",
                    mediaItem.EmbyData.Identifier.Name, mediaItem.ItemType, "Property mapping returned no data"))
                .Match(r => string.IsNullOrWhiteSpace(r.Item.Name)
                        ? Left<ProcessFailedResult, MetadataResult<TEmbyItem>>(
                            new ProcessFailedResult("PropertyMapping", mediaItem.EmbyData.Identifier.Name,
                                mediaItem.ItemType, "Property mapping failed for the Name property"))
                        : Right<ProcessFailedResult, MetadataResult<TEmbyItem>>(r),
                    failure => failure)
                .Map(r => (IMetadataFoundResult<TEmbyItem>)new MetadataFoundResult<TEmbyItem>(mediaItem, r));
        }

        private MetadataResult<TEmbyItem> UpdateProviderIds(MetadataResult<TEmbyItem> metadataResult,
            IEnumerable<ISourceData> sourceData)
        {
            return sourceData.Aggregate(metadataResult, (r, sd) =>
            {
                return sd.Id.Match(id =>
                    {
                        r.Item.SetProviderId(sd.Source.Name, id.ToString());

                        return r;
                    },
                    () =>
                    {
                        if (r.Item.ProviderIds.ContainsKey(sd.Source.Name))
                        {
                            r.Item.ProviderIds.Remove(sd.Source.Name);
                        }

                        return r;
                    });
            });
        }
    }
}