using System.Collections.Generic;
using System.Linq;
using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    /// <summary>
    ///     The data supplied by Emby for an item at the start of the process
    /// </summary>
    internal class EmbyItemData : IEmbyItemData
    {
        private readonly IDictionary<string, int> existingIds;
        private readonly IEnumerable<EmbyItemId> parentIds;

        public EmbyItemData(IMediaItemType itemType, IItemIdentifier identifier, IDictionary<string, int> existingIds,
            string language, IEnumerable<EmbyItemId> parentIds)
        {
            this.parentIds = parentIds;
            ItemType = itemType;
            Identifier = identifier;
            Language = language;
            this.existingIds = existingIds ?? new Dictionary<string, int>();
        }

        public IMediaItemType ItemType { get; }

        /// <summary>
        ///     The identifier provided by Emby
        /// </summary>
        public IItemIdentifier Identifier { get; }

        public string Language { get; }

        /// <summary>
        ///     True if this data came from the file system rather than the Emby library
        /// </summary>
        public bool IsFileData => !this.existingIds.Any();

        /// <summary>
        ///     Get the id that already exists in Emby for a particular source
        /// </summary>
        public Option<int> GetExistingId(string sourceName)
        {
            return !this.existingIds.ContainsKey(sourceName) ? Option<int>.None : this.existingIds[sourceName];
        }

        /// <summary>
        ///     Get the id of the parent of the specified item type for a particular source
        /// </summary>
        public Option<int> GetParentId(IMediaItemType itemType, ISource source)
        {
            var parentId = this.parentIds.Find(id => id.ItemType == itemType && id.SourceName == source.Name);

            return parentId.Map(id => id.Id);
        }

        public override string ToString()
        {
            return $"{ItemType} '{Identifier}'";
        }
    }
}