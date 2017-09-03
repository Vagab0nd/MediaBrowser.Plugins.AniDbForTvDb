using System.Collections.Generic;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb.Series.Data;
using MediaBrowser.Plugins.AniMetadata.Configuration;

namespace MediaBrowser.Plugins.AniMetadata.Providers.AniDb
{
    internal interface ITitleSelector
    {
        Option<ItemTitleData> SelectTitle(IEnumerable<ItemTitleData> titles, TitleType preferredTitleType,
            string metadataLanguage);
    }
}