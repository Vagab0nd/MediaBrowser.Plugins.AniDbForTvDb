using System.Collections.Generic;
using System.Linq;
using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    /// <summary>
    ///     The data supplied by Emby for an item at the start of the process
    /// </summary>
    internal class EmbyItemData
    {
        private readonly IDictionary<string, int> _existingIds;

        public EmbyItemData(IItemIdentifier identifier, IDictionary<string, int> existingIds)
        {
            Identifier = identifier;
            _existingIds = existingIds ?? new Dictionary<string, int>();
        }

        /// <summary>
        ///     The identifier provided by Emby
        /// </summary>
        public IItemIdentifier Identifier { get; }

        /// <summary>
        ///     True if this data came from the file system rather than the Emby library
        /// </summary>
        public bool IsFileData => !_existingIds.Any();

        /// <summary>
        ///     Get the id that already exists in Emby for a particular source
        /// </summary>
        public Option<int> GetExistingId(string sourceName)
        {
            return Option<int>.None;
        }

        /// <summary>
        ///     Get the id of the parent of the specified item type for a particular source
        /// </summary>
        public Option<int> GetParentId(string sourceName, ItemType itemType)
        {
            return Option<int>.None;
        }
    }
}