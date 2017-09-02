using System.Collections.Generic;
using Functional.Maybe;
using MediaBrowser.Plugins.AniMetadata.AniDb.Series.Data;
using MediaBrowser.Plugins.AniMetadata.Configuration;

namespace MediaBrowser.Plugins.AniMetadata.Providers.AniDb
{
    internal interface ITitleSelector
    {
        Maybe<ItemTitleData> SelectTitle(IEnumerable<ItemTitleData> titles, TitleType preferredTitleType,
            string metadataLanguage);
    }
}