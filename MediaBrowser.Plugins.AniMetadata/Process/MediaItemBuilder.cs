using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.SourceDataLoaders;
using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal class MediaItemBuilder : IMediaItemBuilder
    {
        private readonly IPluginConfiguration _pluginConfiguration;
        private readonly IEnumerable<ISourceDataLoader> _sourceDataLoaders;

        public MediaItemBuilder(IPluginConfiguration pluginConfiguration,
            IEnumerable<ISourceDataLoader> sourceDataLoaders)
        {
            _pluginConfiguration = pluginConfiguration;
            _sourceDataLoaders = sourceDataLoaders;
        }

        public Task<Either<ProcessFailedResult, IMediaItem>> IdentifyAsync(EmbyItemData embyItemData,
            IMediaItemType itemType)
        {
            return IdentifyAsync(embyItemData).MapAsync(sd => (IMediaItem)new MediaItem(embyItemData, itemType, sd));
        }

        public Task<Either<ProcessFailedResult, IMediaItem>> BuildMediaItemAsync(IMediaItem rootMediaItem)
        {
            return AddDataFromSourcesAsync(Right<ProcessFailedResult, IMediaItem>(rootMediaItem).AsTask(),
                _sourceDataLoaders.ToImmutableList());

            Task<Either<ProcessFailedResult, IMediaItem>> AddDataFromSourcesAsync(
                Task<Either<ProcessFailedResult, IMediaItem>> mediaItem,
                ImmutableList<ISourceDataLoader> sourceDataLoaders)
            {
                var sourceLoaderCount = sourceDataLoaders.Count;

                var mediaItemTask = sourceDataLoaders.Aggregate(mediaItem,
                    (miTask, l) =>
                        miTask.MapAsync(mi => mi.GetAllSourceData()
                            .Find(l.CanLoadFrom)
                            .MatchAsync(sd => l.LoadFrom(mi, sd)
                                    .Map(e => e.Match(
                                        newSourceData =>
                                        {
                                            sourceDataLoaders = sourceDataLoaders.Remove(l);
                                            return mi.AddData(newSourceData).IfLeft(mi);
                                        },
                                        fail => mi
                                    )),
                                () => mi)));

                return mediaItemTask.BindAsync(mi =>
                {
                    var wasSourceDataAdded = sourceLoaderCount != sourceDataLoaders.Count;

                    var mediaItemAsEither = Right<ProcessFailedResult, IMediaItem>(mi).AsTask();

                    return wasSourceDataAdded
                        ? AddDataFromSourcesAsync(mediaItemAsEither, sourceDataLoaders)
                        : mediaItemAsEither;
                });
            }
        }

        private Task<Either<ProcessFailedResult, ISourceData>> IdentifyAsync(EmbyItemData embyItemData)
        {
            var identifyingSource = embyItemData.IsFileData
                ? _pluginConfiguration.FileStructureSource
                : _pluginConfiguration.LibraryStructureSource;

            return identifyingSource.GetEmbySourceDataLoader(embyItemData.ItemType)
                .BindAsync(l => l.LoadFrom(embyItemData));
        }
    }
}