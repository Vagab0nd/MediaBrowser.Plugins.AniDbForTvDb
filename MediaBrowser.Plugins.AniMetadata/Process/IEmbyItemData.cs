using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal interface IEmbyItemData
    {
        IItemIdentifier Identifier { get; }

        bool IsFileData { get; }

        IMediaItemType ItemType { get; }

        string Language { get; }

        Option<int> GetExistingId(string sourceName);

        Option<int> GetParentId(IMediaItemType itemType, ISource source);
    }
}