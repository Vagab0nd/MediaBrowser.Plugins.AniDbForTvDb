using Functional.Maybe;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.Titles
{
    internal interface ISeriesTitleCache
    {
        Maybe<TitleListItemData> FindSeriesByTitle(string title);
    }
}