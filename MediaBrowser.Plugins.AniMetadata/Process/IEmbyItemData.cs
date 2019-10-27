using LanguageExt;

namespace Emby.AniDbMetaStructure.Process
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