using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using LanguageExt;
using static LanguageExt.Prelude;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal class MediaItem : IMediaItem
    {
        private readonly ImmutableDictionary<ISource, ISourceData> _sourceData;

        /// <summary>
        ///     Create a new <see cref="MediaItem" />
        /// </summary>
        /// <param name="embyData">The name of the itme as originally provided by Emby</param>
        /// <param name="itemType">The type of the media item</param>
        /// <param name="sourceData">The metadata from the source used to initially identify this media item</param>
        public MediaItem(IEmbyItemData embyData, IMediaItemType itemType, ISourceData sourceData)
        {
            if (sourceData == null)
            {
                throw new ArgumentNullException(nameof(sourceData));
            }

            EmbyData = embyData ?? throw new ArgumentNullException(nameof(embyData));
            ItemType = itemType;

            _sourceData = ImmutableDictionary<ISource, ISourceData>.Empty.Add(sourceData.Source, sourceData);
        }

        private MediaItem(IEmbyItemData embyData, IMediaItemType itemType,
            ImmutableDictionary<ISource, ISourceData> sourceData)
        {
            EmbyData = embyData;
            ItemType = itemType;
            _sourceData = sourceData;
        }

        public IEmbyItemData EmbyData { get; }

        public IMediaItemType ItemType { get; }

        public Option<ISourceData> GetDataFromSource(ISource source)
        {
            _sourceData.TryGetValue(source, out var sourceData);

            return Option<ISourceData>.Some(sourceData);
        }

        public IEnumerable<ISourceData> GetAllSourceData()
        {
            return _sourceData.Values;
        }

        public Either<ProcessFailedResult, IMediaItem> AddData(ISourceData sourceData)
        {
            if (_sourceData.ContainsKey(sourceData.Source))
            {
                var failedResult = new ProcessFailedResult(sourceData.Source.Name, sourceData.Identifier.Name, ItemType,
                    "Cannot add data for a source more than once");

                return Left<ProcessFailedResult, IMediaItem>(failedResult);
            }

            return Right<ProcessFailedResult, IMediaItem>(new MediaItem(EmbyData, ItemType,
                _sourceData.Add(sourceData.Source, sourceData)));
        }
    }
}