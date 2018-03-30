using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal class ItemIdentifier : IItemIdentifier
    {
        public ItemIdentifier(Option<int> index, Option<int> parentIndex, string name)
        {
            Index = index;
            ParentIndex = parentIndex;
            Name = name;
        }

        public Option<int> Index { get; }

        public Option<int> ParentIndex { get; }

        public string Name { get; }

        public override string ToString()
        {
            return
                $"Name: '{Name}' Index: '{Index.Map(i => i.ToString()).IfNone("")}', ParentIndex: '{ParentIndex.Map(i => i.ToString()).IfNone("")}'";
        }
    }
}