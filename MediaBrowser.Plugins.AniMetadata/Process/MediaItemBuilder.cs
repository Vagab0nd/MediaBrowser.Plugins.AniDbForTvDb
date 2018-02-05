using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using static LanguageExt.Prelude;

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

        public Task<Either<ProcessFailedResult, IMediaItem>> IdentifyAsync(EmbyItemData embyItemData,
            IMediaItemType itemType)
        {
            return IdentifyAsync(embyItemData).MapAsync(sd => (IMediaItem)new MediaItem(embyItemData, itemType, sd));
        }

        public Task<Either<ProcessFailedResult, IMediaItem>> BuildMediaItemAsync(IMediaItem rootMediaItem)
        {
            return AddDataFromSourcesAsync(Right<ProcessFailedResult, IMediaItem>(rootMediaItem).AsTask(),
                _sources.Where(s => rootMediaItem.GetDataFromSource(s).IsNone).ToImmutableList());

            Task<Either<ProcessFailedResult, IMediaItem>> AddDataFromSourcesAsync(
                Task<Either<ProcessFailedResult, IMediaItem>> mediaItem, ImmutableList<ISource> sources)
            {
                var sourceCount = sources.Count;

                var mediaItemTask = sources.Aggregate(mediaItem,
                    (miTask, s) =>
                        miTask.BindAsync(mi => s.LookupAsync(mi)
                            .Bind(e =>
                            {
                                if (e.IsLeft)
                                {
                                    return Right<ProcessFailedResult, IMediaItem>(mi).AsTask();
                                }

                                return e.BindAsync(sd =>
                                {
                                    sources = sources.Remove(s);
                                    return mi.AddData(sd).AsTask();
                                });
                            })));

                return mediaItemTask.BindAsync(mi =>
                {
                    var wasSourceDataAdded = sourceCount != sources.Count;

                    var mediaItemAsEither = Right<ProcessFailedResult, IMediaItem>(mi).AsTask();

                    return wasSourceDataAdded ? AddDataFromSourcesAsync(mediaItemAsEither, sources) : mediaItemAsEither;
                });
            }
        }

        private Task<Either<ProcessFailedResult, ISourceData>> IdentifyAsync(EmbyItemData embyItemData)
        {
            return embyItemData.IsFileData
                ? _pluginConfiguration.FileStructureSource.LookupAsync(embyItemData)
                : _pluginConfiguration.LibraryStructureSource.LookupAsync(embyItemData);
        }
    }
}