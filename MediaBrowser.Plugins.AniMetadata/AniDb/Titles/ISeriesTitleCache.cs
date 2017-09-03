using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.Titles
{
    internal interface ISeriesTitleCache
    {
        Option<TitleListItemData> FindSeriesByTitle(string title);
    }
}