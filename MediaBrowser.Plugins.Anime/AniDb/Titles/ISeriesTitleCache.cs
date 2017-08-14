using Functional.Maybe;

namespace MediaBrowser.Plugins.Anime.AniDb.Titles
{
    internal interface ISeriesTitleCache
    {
        Maybe<TitleListItemData> FindSeriesByTitle(string title);
    }
}