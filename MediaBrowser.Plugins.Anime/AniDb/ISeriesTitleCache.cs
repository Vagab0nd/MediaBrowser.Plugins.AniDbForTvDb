using Functional.Maybe;
using MediaBrowser.Plugins.Anime.AniDb.Data;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    internal interface ISeriesTitleCache
    {
        Maybe<TitleListItemData> FindSeriesByTitle(string title);
    }
}