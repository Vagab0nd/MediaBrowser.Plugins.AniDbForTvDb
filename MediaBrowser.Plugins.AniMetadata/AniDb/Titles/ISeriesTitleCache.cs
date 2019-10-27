using LanguageExt;

namespace Emby.AniDbMetaStructure.AniDb.Titles
{
    internal interface ISeriesTitleCache
    {
        Option<TitleListItemData> FindSeriesByTitle(string title);
    }
}