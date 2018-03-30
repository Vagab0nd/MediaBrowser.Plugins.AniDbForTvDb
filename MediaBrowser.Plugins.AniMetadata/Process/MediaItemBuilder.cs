using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Model.Logging;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.SourceDataLoaders;
using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal class MediaItemBuilder : IMediaItemBuilder
    {
        private readonly IPluginConfiguration _pluginConfiguration;
        private readonly IEnumerable<ISourceDataLoader> _sourceDataLoaders;
        private readonly ILogger _log;

        public MediaItemBuilder(IPluginConfiguration pluginConfiguration,
            IEnumerable<ISourceDataLoader> sourceDataLoaders, ILogManager logManager)
        {
            _pluginConfiguration = pluginConfiguration;
            _sourceDataLoaders = sourceDataLoaders;
            _log = logManager.GetLogger(nameof(MediaItemBuilder));
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
                        miTask.MapAsync(mi => mi.GetAllSourceData().Find(l.CanLoadFrom)
                            .MatchAsync(sd =>
                                {
                                    _log.Debug($"Loading source data using {l.GetType().FullName}");
                                    return l.LoadFrom(mi, sd)
                                        .Map(e => e.Match(
                                            newSourceData =>
                                            {
                                                _log.Debug($"Loaded {sd.Source.Name} source data: {sd.Identifier}");
                                                sourceDataLoaders = sourceDataLoaders.Remove(l);
                                                return mi.AddData(newSourceData).IfLeft(() =>
                                                {
                                                    _log.Warn($"Failed to add source data: {sd.Identifier}");
                                                    return mi;
                                                });
                                            },
                                            fail =>
                                            {
                                                _log.Debug($"Failed to load source data: {fail.Reason}");
                                                return mi;
                                            }));
                                },
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