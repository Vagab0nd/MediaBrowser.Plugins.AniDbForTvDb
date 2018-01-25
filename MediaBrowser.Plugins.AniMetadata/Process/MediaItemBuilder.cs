using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal class MediaItemBuilder : IMediaItemBuilder
    {
        private readonly INewPluginConfiguration _pluginConfiguration;
        private readonly IEnumerable<ISource> _sources;

        public MediaItemBuilder(INewPluginConfiguration pluginConfiguration, IEnumerable<ISource> sources)
        {
            _pluginConfiguration = pluginConfiguration;
            _sources = sources;
        }

        public Option<IMediaItem> Identify(EmbyItemData embyItemData, ItemType itemType)
        {
            return Identify(embyItemData).Map<IMediaItem>(sd => new MediaItem(itemType, sd));
        }

        public IMediaItem BuildMediaItem(IMediaItem rootMediaItem)
        {
            return AddDataFromSources(rootMediaItem,
                _sources.Where(s => rootMediaItem.GetDataFromSource(s).IsNone).ToImmutableList());

            IMediaItem AddDataFromSources(IMediaItem mediaItem, ImmutableList<ISource> sources)
            {
                var sourceCount = sources.Count;

                mediaItem = sources.Aggregate(mediaItem,
                    (mi, s) =>
                        s.Lookup(mediaItem)
                            .Match(sd =>
                                {
                                    sources = sources.Remove(s);
                                    return mi.AddData(sd);
                                },
                                () => mi));

                var wasSourceDataAdded = sourceCount != sources.Count;

                return wasSourceDataAdded ? AddDataFromSources(mediaItem, sources) : mediaItem;
            }
        }

        private Option<ISourceData> Identify(EmbyItemData embyItemData)
        {
            return embyItemData.IsFileData
                ? _pluginConfiguration.FileStructureSource.Lookup(embyItemData)
                : _pluginConfiguration.LibraryStructureSource.Lookup(embyItemData);
        }
    }
}