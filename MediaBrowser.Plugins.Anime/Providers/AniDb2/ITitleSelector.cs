using System.Collections.Generic;
using Functional.Maybe;
using MediaBrowser.Plugins.Anime.AniDb.Data;
using MediaBrowser.Plugins.Anime.Configuration;

namespace MediaBrowser.Plugins.Anime.Providers.AniDb2
{
    internal interface ITitleSelector
    {
        Maybe<ItemTitleData> SelectTitle(IEnumerable<ItemTitleData> titles, TitlePreferenceType preferredTitleType, string metadataLanguage);
    }
}