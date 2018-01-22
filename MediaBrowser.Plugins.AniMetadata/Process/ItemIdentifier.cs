namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal class ItemIdentifier : IItemIdentifier
    {
        public ItemIdentifier(int? index, int? parentIndex, string name)
        {
            Index = index;
            ParentIndex = parentIndex;
            Name = name;
        }

        public int? Index { get; }

        public int? ParentIndex { get; }

        public string Name { get; }
    }
}