using System.Collections.Generic;
using MediaBrowser.Plugins.Anime.AniDb.Data;
using MediaBrowser.Plugins.Anime.Configuration;

namespace MediaBrowser.Plugins.Anime.Providers.AniDb2
{
    internal interface ITitleSelector
    {
        IOption<ItemTitle> SelectTitle(IEnumerable<ItemTitle> titles, TitlePreferenceType preferredTitleType, string metadataLanguage);
    }
}