using System.Collections.Generic;
using Emby.AniDbMetaStructure.AniDb.SeriesData;
using Emby.AniDbMetaStructure.Configuration;
using LanguageExt;

namespace Emby.AniDbMetaStructure.AniDb
{
    internal interface IAniDbTitleSelector
    {
        Option<ItemTitleData> SelectTitle(IEnumerable<ItemTitleData> titles, TitleType preferredTitleType,
            string metadataLanguage);
    }
}