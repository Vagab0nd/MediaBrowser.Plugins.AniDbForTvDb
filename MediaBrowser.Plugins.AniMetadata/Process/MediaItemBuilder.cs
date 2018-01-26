using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
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

        public OptionAsync<IMediaItem> IdentifyAsync(EmbyItemData embyItemData, ItemType itemType)
        {
            return Identify(embyItemData).Map<IMediaItem>(sd => new MediaItem(itemType, sd));
        }

        public Task<IMediaItem> BuildMediaItemAsync(IMediaItem rootMediaItem)
        {
            return AddDataFromSourcesAsync(rootMediaItem,
                _sources.Where(s => rootMediaItem.GetDataFromSource(s).IsNone).ToImmutableList());

            Task<IMediaItem> AddDataFromSourcesAsync(IMediaItem mediaItem, ImmutableList<ISource> sources)
            {
                var sourceCount = sources.Count;

                var mediaItemTask = sources.Aggregate(mediaItem.AsTask(),
                    (miTask, s) =>
                        miTask.Bind(mi =>
                            s.LookupAsync(mi)
                                .Match(sd =>
                                    {
                                        sources = sources.Remove(s);
                                        return mi.AddData(sd);
                                    },
                                    () => mi)));

                return mediaItemTask.Bind(mi =>
                {
                    var wasSourceDataAdded = sourceCount != sources.Count;

                    return wasSourceDataAdded ? AddDataFromSourcesAsync(mi, sources) : mi.AsTask();
                });
            }
        }

        private OptionAsync<ISourceData> Identify(EmbyItemData embyItemData)
        {
            return embyItemData.IsFileData
                ? _pluginConfiguration.FileStructureSource.LookupAsync(embyItemData)
                : _pluginConfiguration.LibraryStructureSource.LookupAsync(embyItemData);
        }
    }
}